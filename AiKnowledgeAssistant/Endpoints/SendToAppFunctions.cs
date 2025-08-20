using AiKnowledgeAssistant.Services.Azure.AppFunction;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AiKnowledgeAssistant.Endpoints;

public static class SendToAppFunctions
{
    public static void MapSendBlobToAzureFunctions(this IEndpointRouteBuilder app)
    {
        app.MapPost("/sendToFunction", async Task<Results<Ok<IFormFile>, BadRequest<string>, ProblemHttpResult>> (
            HttpRequest request,
            IApplicationFunctionService funcService,
            TelemetryClient telemetry,
            CancellationToken ct) =>
        {
            try
            {
                telemetry.TrackEvent("Send request with blob object to azure functions");

                if (!request.HasFormContentType)
                {
                    telemetry.TrackEvent("Sending blob object failed - not valid form content type", new Dictionary<string, string>
                    {
                        [request.RouteValues.ToString() ?? "Failed fetch route values"] = $"Http route values"
                    });

                    return TypedResults.BadRequest("No file provided");
                }

                var form = await request.ReadFormAsync(ct);
                var formFile = form.Files.FirstOrDefault();

                if (formFile is null)
                {
                    telemetry.TrackEvent("Sending blob object failed - form file not found", new Dictionary<string, string>
                    {
                        [request.Headers.ContentLocation.ToString() ?? "Failed fetch fomr request headers"] = $"Http route headers"
                    });

                    return TypedResults.BadRequest("Form not found");
                }

                var allowedTypes = new[] { "application/pdf", "image/png" };

                if (!allowedTypes.Contains(formFile.ContentType))
                {
                    return TypedResults.BadRequest("Unsupported file type");
                }

                using var copyStream = formFile.OpenReadStream();

                telemetry.TrackEvent("Uploading blob to azure functions", new Dictionary<string, string>
                {
                    ["FileName"] = formFile.FileName
                });

                telemetry.TrackEvent("Activation copying file body function");
                await funcService.UploadCopyBlobAsync(formFile.FileName, copyStream, ct);

                using var metadataStream = formFile.OpenReadStream();

                telemetry.TrackEvent("Activation extract metadata function from file");
                await funcService.UploadExtractBlobMetadataAsync(formFile.FileName, metadataStream, ct);

                telemetry.TrackEvent("Success");
                return TypedResults.Ok(formFile);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex, new Dictionary<string, string>
                {
                    [ex.Message] = "Exception Message",
                    [ex.Source ?? "No source found"] = "Exception Source"
                });

                return TypedResults.Problem(
                    ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError)
        .DisableAntiforgery()
        .WithName("SendToAzureFunctions")
        .WithTags("Files")
        .WithOpenApi();
    }
}