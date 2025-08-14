using AiKnowledgeAssistant.Services.Azure.BlobStorage;

namespace AiKnowledgeAssistant.Endpoints;

public static class UploadBlob
{
    public static void MapUploadBlobEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/upload", async (HttpRequest request, BlobStorageService blobService) =>
        {
            var formFile = request.Form.Files.FirstOrDefault();
            if (formFile is null)
            {
                return Results.BadRequest("No file uploaded");
            }

            using var stream = formFile.OpenReadStream();
            await blobService.UploadAsync(formFile.FileName, stream);

            return Results.Ok("File uploaded successfully");
        })
        .WithName("UploadFile")
        .WithTags("Files")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
