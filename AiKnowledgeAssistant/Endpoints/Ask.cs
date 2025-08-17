using AiKnowledgeAssistant.Services.Azure.KeyVault;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models.Embedding;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models.GPT;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AiKnowledgeAssistant.Endpoints
{
    public static class Ask
    {
        public static void MapAskEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/ask", async Task<Results<Ok<object>, BadRequest<string>, ProblemHttpResult>> (
                HttpRequest request,
                TextEmbeddingService embeddingService,
                IChatService chatService,
                ISecretProvider secretClient,
                TelemetryClient telemetryClient) =>
            {
                try
                {
                    using var reader = new StreamReader(request.Body);

                    string body = await reader.ReadToEndAsync();

                    if (string.IsNullOrWhiteSpace(body))
                    {
                        return TypedResults.BadRequest("No question provided");
                    }

                    string question = body;

                    // generate embedding questions
                    float[] questionEmbedding = await embeddingService.GetEmbeddingAsync(question);

                    // get cognitive search
                    string? searchEndpoint = await secretClient.GetSecretValueAsync("Azure--search-endpoint");
                    ArgumentException.ThrowIfNullOrEmpty(searchEndpoint, nameof(searchEndpoint));
                    string? searchKey = await secretClient.GetSecretValueAsync("Azure--search-key");
                    ArgumentException.ThrowIfNullOrEmpty(searchKey, nameof(searchKey));

                    var searchClient = new SearchClient(
                        new Uri(searchEndpoint),
                        "documents-index",
                        new AzureKeyCredential(searchKey));

                    // vector search - cognitive search
                    var options = new SearchOptions
                    {
                        VectorSearch = new()
                        {
                            Queries =
                            {
                                new VectorizedQuery(questionEmbedding)
                                {
                                    KNearestNeighborsCount = 3,
                                    Fields = { "embedding" }
                                }
                            }

                        }
                    };

                    var searchResponse = await searchClient.SearchAsync<SearchDocument>(null, options);

                    var context = string.Join("\n---\n", searchResponse.Value.GetResults()
                        .Select(r => r.Document["content"].ToString()));

                    if (string.IsNullOrWhiteSpace(context))
                    {
                        return TypedResults.Ok<object>(new { question, answer = "No documents matching the question." });
                    }

                    var prompt = $@"
                        Answer the user's question using only the following document excerpts.
                        If the answer isn't in the documents, say you don't know.

                        Fragments:
                        {context}

                        Question:
                        {question}";

                    var answer = await chatService.AskAsync(prompt);

                    telemetryClient.TrackEvent("QuestionAnswered", new Dictionary<string, string>
                    {
                        ["Question"] = question,
                        ["AnswerLength"] = answer.Length.ToString()
                    });

                    return TypedResults.Ok<object>(new
                    {
                        question,
                        answer
                    });
                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);

                    return TypedResults.Problem(
                        detail: $"Error occurred while answering question: {ex.Message}",
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Internal error"
                    );
                }
            })
            .WithName("AskQuestion")
            .WithTags("QnA")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();
        }
    }
}
