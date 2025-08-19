namespace AiKnowledgeAssistant.Services.Azure.AppFunction;

public interface IApplicationFunctionService
{
    Task UploadExtractBlobMetadataAsync(string blobName, Stream stream, CancellationToken ct);
    Task UploadCopyBlobAsync(string blobName, Stream stream, CancellationToken ct);
}
