using AiKnowledgeAssistant.Services.Azure.BlobStorage;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AiKnowledgeAssistant.Endpoints;

public static class UploadBlob
{
    public static void MapUploadBlobEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/upload", async Task<Results<Ok<string>, BadRequest<string>, ProblemHttpResult>> (
            [FromForm] IFormFile? formFile,
            IBlobStorageService blobService,
            TelemetryClient telemetryClient) =>
        {
            try
            {
                DateTime start = DateTime.UtcNow;
                var sw = new Stopwatch();
                sw.Start();

                if (formFile is null || formFile.Length == 0)
                {
                    sw.Stop();

                    telemetryClient.TrackRequest(
                       name: "Upload Blob failure, no object provided",
                       startTime: start,
                       duration: TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds),
                       StatusCodes.Status400BadRequest.ToString(),
                       false);

                    return TypedResults.BadRequest("No file uploaded");
                }

                using var stream = formFile.OpenReadStream();
                await blobService.UploadAsync(formFile.FileName, stream);

                sw.Stop();

                telemetryClient.TrackRequest(
                    name: "Upload Blob to Azure Blob Storage",
                    startTime: start,
                    duration: TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds),
                    StatusCodes.Status200OK.ToString(),
                    true);

                return TypedResults.Ok("File uploaded successfully");
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
                    detail: $"Error ocurred while uploading a file: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal error"
                    );
            }
        })
        .WithName("UploadFile")
        .WithTags("Files")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .DisableAntiforgery()
        .WithOpenApi();
    }
}
