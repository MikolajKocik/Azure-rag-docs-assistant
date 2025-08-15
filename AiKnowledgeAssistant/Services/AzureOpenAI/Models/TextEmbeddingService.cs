using Azure;
using Azure.AI.OpenAI;
using DotNetEnv;
using OpenAI;
using OpenAI.Embeddings;

namespace AiKnowledgeAssistant.Services.AzureOpenAI.Models;

public sealed class TextEmbeddingService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly string _deploymentName = "text-embedding-ada-002";

    public TextEmbeddingService(IConfiguration cfg, WebApplicationBuilder builder)  
    {
        var endpoint = new Uri(builder.Configuration["AZURE_OPENAI_ENDPOINT"]
            ?? throw new ArgumentException("Azure OpenAI endpoint not found"));

        var apiKey = builder.Configuration["AZURE_OPENAI_KEY"]
            ?? throw new ArgumentException("Azure OpenAI api key not found");


        _openAIClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
    }

    public async Task<float[]> GetEmbeddingAsync(string input)
    {
        EmbeddingClient embeddingClient = _openAIClient.GetEmbeddingClient(_deploymentName);
        var response = await embeddingClient.GenerateEmbeddingAsync(input);
        return response.Value.ToFloats().ToArray();
    }
}
