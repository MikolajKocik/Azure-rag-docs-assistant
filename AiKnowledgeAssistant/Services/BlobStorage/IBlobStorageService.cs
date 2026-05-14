namespace AiKnowledgeAssistant.Services.BlobStorage;

public interface IBlobStorageService
{
    Task UploadAsync(string fileName, Stream fileStream, CancellationToken cancellationToken);
    Task UploadExtractBlobMetadataAsync(string blobName, Stream stream, CancellationToken cancellationToken);
    Task UploadCopyBlobAsync(string blobName, Stream stream, CancellationToken cancellationToken);
}
