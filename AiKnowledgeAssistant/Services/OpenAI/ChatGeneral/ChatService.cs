using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;
using AiKnowledgeAssistant.Services.OpenAI.Ranker;
using AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;
using AiKnowledgeAssistant.Services.OpenAI.ChatEmbeddings;
using Microsoft.Extensions.Options;
using AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;
using System.Diagnostics;

namespace AiKnowledgeAssistant.Services.OpenAI.ChatGeneral;

public sealed class ChatService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChunkRanker _chunkRanker;
    private readonly SearchClient _searchClient;
    private readonly int _finalTopK;
    private readonly int _vectorTopK;

    public ChatService(
        ITextEmbeddingService embeddingService,
        IChunkRanker chunkRanker,
        AzureOpenAIClient client,
        SearchClient searchClient,
        IOptions<RagOptions> ragOptions,
        IOptions<AzureOpenAIOptions> azOptions
        )
    {
        _chatClient = client.GetChatClient(azOptions.Value.ChatDeploymentName);

        _vectorTopK = ragOptions.Value.VectorTopK;
        _finalTopK = ragOptions.Value.FinalTopK;

        _embeddingService = embeddingService;
        _chunkRanker = chunkRanker;
        _searchClient = searchClient;
    }

    /// <summary>
    /// Retrieves relevant document excerpts from Azure AI Search based on the user's question embedding.
    /// </summary>
    /// <param name="request">The chat request containing the user's question.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A formatted string containing the concatenated content of the top matching documents.</returns>
    public async Task<RetrievalResult> RetrieveDocumentAsync(string question, CancellationToken cancellationToken)
    {   
        var stopwatch = Stopwatch.StartNew();

        float[] questionEmbedding = await _embeddingService.GetEmbeddingAsync(question, cancellationToken);

        // Cognitive Search (vector search)
        var options = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(questionEmbedding)
                    {
                        KNearestNeighborsCount = _vectorTopK,
                        Fields = { "embedding" }
                    }
                }
            } 
        };
        
        Response<SearchResults<SearchDocument>> searchResponse = 
            await _searchClient.SearchAsync<SearchDocument>(
                null, 
                options, 
                cancellationToken
            );
        
        List<string> chunks = searchResponse.Value
            .GetResults()
            .Select(result => result.Document["content"].ToString()!)
            .Where(content => !string.IsNullOrWhiteSpace(content))
            .ToList();

        IReadOnlyList<RankedChunk> rankedChunks = await _chunkRanker.RankAsync(
            question,
            chunks,
            cancellationToken
        );

        IReadOnlyList<RankedChunk> selectedChunks = rankedChunks
            .Take(_finalTopK)
            .ToList();
          
        string context = string.Join(
            "\n---\n", 
            selectedChunks.Select(x => x.Content)
        );

        stopwatch.Stop();

        return new RetrievalResult(
            context,
            rankedChunks,
            selectedChunks,
            _vectorTopK,
            _finalTopK,
            stopwatch.ElapsedMilliseconds
        );
    }

    /// <summary>
    /// Generates an answer to the user's question using the Azure OpenAI GPT model, augmented with the retrieved document context.
    /// </summary>
    /// <param name="request">The chat request containing the user's question.</param>
    /// <param name="context">The document excerpts retrieved from the knowledge base to be used as context.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The generated text response from the GPT model.</returns>
    public async Task<string> AskAsync(string question, string context, CancellationToken cancellationToken)
    {
        var requestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = 4096,
            Temperature = 0.1f,
        };

        string systemMsg = $@"
        You are a helpful knowledge document assistant.

        Instructions:
        - Answer the user's question using ONLY the following document excerpts.
        - If the answer isn't in the documents, say 'Sorry but I dont have any information about this question.'

        Documents (Context):
        {context}
        ";

        List<ChatMessage> messages = new()
        {
            new SystemChatMessage(systemMsg),
            new UserChatMessage(question),
        };

        ClientResult<ChatCompletion> response = await _chatClient.CompleteChatAsync(messages, requestOptions, cancellationToken);
        return response.Value.Content[0].Text;
    }
}
