using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Embeddings;

namespace AiKnowledgeAssistant.Services.OpenAI;

public sealed class TextEmbeddingService : ITextEmbeddingService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly IConfiguration _cfg;
    private readonly string _deploymentName = "text-embedding-ada-002";

    public TextEmbeddingService(IConfiguration cfg)
    {
        _cfg = cfg;

        var endpoint = new Uri(_cfg["AZURE_OPENAI_ENDPOINT"]
            ?? throw new ArgumentException("Azure OpenAI endpoint not found"));

        var apiKey = _cfg["AZURE_OPENAI_KEY"]
            ?? throw new ArgumentException("Azure OpenAI api key not found");

        _openAIClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
    }
  

    public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken)
    {
        EmbeddingClient embeddingClient = _openAIClient.GetEmbeddingClient(_deploymentName);
        ClientResult<OpenAIEmbedding> response = await embeddingClient.GenerateEmbeddingAsync(input, null, cancellationToken);
        return response.Value.ToFloats().ToArray();
    }
}
