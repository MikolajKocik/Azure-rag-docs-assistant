using AiKnowledgeAssistant.Services.OpenAI;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AiKnowledgeAssistant.Endpoints
{
    public static class ProcessDocument
    {
        public static void MapFormRecognizerEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/process", async Task<Results<Ok<object>, BadRequest<string>, ProblemHttpResult>> (
                IFormCollection form,
                IIngestionService recognizerService,
                TelemetryClient telemetryClient,
                CancellationToken cancellationToken
                ) =>
            {
                try
                {   
                    IFormFile formFile = form.Files.FirstOrDefault();

                    if (formFile is null)
                    {
                        return TypedResults.Problem(
                            detail: "File is empty",
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Bad request"
                        );
                    }

                    await recognizerService.InitializeAsync(cancellationToken);
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
