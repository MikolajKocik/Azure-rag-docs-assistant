using AiKnowledgeAssistant.Services.Azure.BlobStorage;
using AiKnowledgeAssistant.Services.Azure.KeyVault;
using AiKnowledgeAssistant.Services.AzureOpenAI.FormRecognizer;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models.Embedding;
using AiKnowledgeAssistant.Utils;
using Azure;
using Azure.Search.Documents;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;

namespace AiKnowledgeAssistant.Endpoints
{
    public static class ProcessDocument
    {
        public static void MapFormRecognizerEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/process", async Task<Results<Ok<object>, BadRequest<string>, ProblemHttpResult>> (
                HttpRequest request,
                IBlobStorageService blobService,
                FormRecognizerService recognizerService,
                TelemetryClient telemetryClient,
                TextEmbeddingService embeddingService,
                ISecretProvider secretClient) =>
            {
                try
                {
                    DateTime start = DateTime.UtcNow;
                    var sw = new Stopwatch();
                    sw.Start();

                    IFormFile? formFile = request.Form.Files.FirstOrDefault();

                    if (formFile is null)
                    {
                        sw.Stop();

                        telemetryClient.TrackRequest(
                            name: "Upload Blob failure, no file uploaded",
                            startTime: start,
                            duration: TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds),
                            StatusCodes.Status400BadRequest.ToString(),
                            false);

                        return TypedResults.BadRequest("No file uploaded");
                    }

                    using var stream = formFile.OpenReadStream();

                    await blobService.UploadAsync(formFile.FileName, stream);

                    stream.Position = 0;
                    string extractedText = await recognizerService.ExtractTextAsync(stream);

                    List<string> chunks = TextChunking.ChunkText(extractedText, 1000);

                    var embeddings = new List<float[]>();

                    foreach(var chunk in chunks)
                    {
                        var vector = await embeddingService.GetEmbeddingAsync(chunk);
                        embeddings.Add(vector);
                    }

                    string? searchEndpoint = await secretClient.GetSecretValueAsync("Azure--search-endpoint");
                    ArgumentException.ThrowIfNullOrEmpty(searchEndpoint, nameof(searchEndpoint));
                    string? searchKey = await secretClient.GetSecretValueAsync("Azure--search-key");
                    ArgumentException.ThrowIfNullOrEmpty(searchKey, nameof(searchKey));

                    var searchClient = new SearchClient(
                        new Uri(searchEndpoint),
                        "documents-index",
                        new AzureKeyCredential(searchKey));

                    var documents = chunks.Select((chunk, idx) => new
                    {
                        id = Guid.NewGuid().ToString(),
                        content = chunk,
                        embedding = embeddings[idx]
                    });

                    await searchClient.UploadDocumentsAsync(documents);

                    return TypedResults.Ok<object>(new
                    {
                        File = formFile.FileName,
                        TextPreview = extractedText.Substring(0, Math.Min(200, extractedText.Length)) + "...",
                        Length = extractedText.Length
                    });
                }
                catch (Exception ex)
                {
                    var exError = new Dictionary<string, string>
                    {
                        ["ExceptionMessage"] = ex.Message,
                        ["IsDataReadOnly"] = ex.Data.IsReadOnly.ToString(),
                        ["ExceptionTarget"] = ex.TargetSite?.Name?.ToString()!
                    };
                    telemetryClient.TrackException(ex, exError.AsReadOnly());

                    return TypedResults.Problem(
                        detail: $"Error ocurred while processing a file: {ex.Message}",
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Internal error"
                    );
                }
            })
            .WithName("ProcessDocument")
            .WithTags("Document")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();
        }
    }
}
