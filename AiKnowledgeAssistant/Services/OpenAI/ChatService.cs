using System.ClientModel;
using AiKnowledgeAssistant.Services.KeyVault;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;

namespace AiKnowledgeAssistant.Services.OpenAI;

public sealed class ChatService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly ILocalRankerService _rerankerService;
    private readonly ISecretProvider _secretClient;

    public ChatService(IConfiguration config, ITextEmbeddingService embeddingService, ILocalRankerService rerankerService, ISecretProvider secretClient)
    {
        var endpoint = new Uri(config["AZURE_OPENAI_ENDPOINT"]
          ?? throw new ArgumentException("Azure OpenAI endpoint not found"));

        string apiKey = config["AZURE_OPENAI_KEY"]
             ?? throw new ArgumentException("Azure OpenAI api key not found");
        string deploymentName = "gpt-4-chat";

        var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        _chatClient = azureClient.GetChatClient(deploymentName);

        _embeddingService = embeddingService;
        _rerankerService = rerankerService;
        _secretClient = secretClient;
    }

    /// <summary>
    /// Retrieves relevant document excerpts from Azure AI Search based on the user's question embedding.
    /// </summary>
    /// <param name="request">The chat request containing the user's question.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A formatted string containing the concatenated content of the top matching documents.</returns>
    public async Task<string> RetrieveDocumentAsync(string document, CancellationToken cancellationToken)
    {   
        float[] questionEmbedding = await _embeddingService.GetEmbeddingAsync(document, cancellationToken);

        string? searchEndpoint = await _secretClient.GetSecretValueAsync("Azure--search-endpoint");
        ArgumentException.ThrowIfNullOrEmpty(searchEndpoint, nameof(searchEndpoint));
            
        string? searchKey = await _secretClient.GetSecretValueAsync("Azure--search-key");           
        ArgumentException.ThrowIfNullOrEmpty(searchKey, nameof(searchKey));

        var searchClient = new SearchClient(
            new Uri(searchEndpoint),
            "documents-index",
            new AzureKeyCredential(searchKey)
        );

        // Cognitive Search (vector search)
        var options = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(questionEmbedding)
                    {
                        KNearestNeighborsCount = 50,
                        Fields = { "embedding" }
                    }
                }
            } 
        };
        
        Response<SearchResults<SearchDocument>> searchResponse = 
            await searchClient.SearchAsync<SearchDocument>(null, options, cancellationToken);

        var scoredResults = new List<(string Content, float Score)>();
        foreach (var result in searchResponse.Value.GetResults())
        {
            string content = result.Document["content"].ToString()!;
            float score = _rerankerService.CalculateScore(document, content);
            scoredResults.Add((content, score));
        }

        var top3Documents = scoredResults
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Content);
          
        return string.Join("\n---\n", top3Documents);
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
