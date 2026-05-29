using AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace AiKnowledgeAssistant.Services.OpenAI.Ranker;

public sealed class LocalRankerService : IChunkRanker, IDisposable
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;

    public LocalRankerService(string modelPath, string tokenizerPath)
    {
        _session = new InferenceSession(modelPath);

        var bpeOptions = new BpeOptions(tokenizerPath);
        _tokenizer = BpeTokenizer.Create(bpeOptions);
    }
    
    private float CalculateScore(string query, string document)
    {
        IReadOnlyList<int> queryTokens = _tokenizer.EncodeToIds(query);
        IReadOnlyList<int> docTokens = _tokenizer.EncodeToIds(document);

        // Format: <s> Question </s></s> Document </s>
        // <s> = 0 | </s> = 2
        var inputIdsList = new List<int> { 0 };
        inputIdsList.AddRange(queryTokens);
        inputIdsList.Add(2);
        inputIdsList.Add(2);
        inputIdsList.AddRange(docTokens);
        inputIdsList.Add(2);

        int sequenceLength = inputIdsList.Count;

        // Tensors
        var inputIdsTensor = new DenseTensor<long>(new[] { 1, sequenceLength });
        var attentionMaskTensor = new DenseTensor<long>(new[] { 1, sequenceLength });

        for (int i = 0; i < sequenceLength; i++)
        {
            inputIdsTensor[0, i] = inputIdsList[i];
            attentionMaskTensor[0, i] = 1; 
        }

        // Neuron network ports
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        using var results = _session.Run(inputs);
        Tensor<float> outputTensor = results.First().AsTensor<float>();

        return Sigmoid(outputTensor[0]);
    }

    private static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    public Task<IReadOnlyList<RankedChunk>> RankAsync(
        string query, 
        IReadOnlyList<string> chunks, 
        CancellationToken cancellationToken
        )
    {
        List<RankedChunk> ranked = chunks
            .Select(chunk =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var score = CalculateScore(query, chunk);
                return new RankedChunk(
                    chunk, 
                    score, 
                    "local_reranker"
                );
            })
            .OrderByDescending(x => x.Score)
            .ToList();
        
        return Task.FromResult<IReadOnlyList<RankedChunk>>(ranked);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
