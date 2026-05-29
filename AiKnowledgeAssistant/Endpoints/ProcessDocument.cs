using AiKnowledgeAssistant.Services.OpenAI.DataIngestion;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeAssistant.Endpoints
{
    public static class ProcessDocument
    {
        public static void MapFormRecognizerEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/process", async Task<Results<Ok<object>, BadRequest<string>, ProblemHttpResult>> (
                [FromForm] IFormFile formFile,
                [FromServices] IIngestionService recognizerService,
                [FromServices] TelemetryClient telemetryClient,
                CancellationToken cancellationToken
                ) =>
            {
                try
                {
                    if (formFile.Length == 0)
                    {
                        return TypedResults.BadRequest("File is empty");
                    }

                    string extractedText = await recognizerService.ProcessDocumentAsync(formFile, cancellationToken);

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
                        ["ExceptionTarget"] = ex.TargetSite?.Name?.ToString() ?? string.Empty
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
            .DisableAntiforgery()
            .WithOpenApi();
        }
    }
}
