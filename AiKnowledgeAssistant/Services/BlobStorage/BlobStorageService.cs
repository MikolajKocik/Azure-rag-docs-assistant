using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.BlobStorage;

public sealed class BlobStorageService(
    BlobStorageClients clients) : IBlobStorageService
{
    private readonly BlobContainerClient _primaryContainer = clients.PrimaryContainer;
    private readonly BlobContainerClient _extractContainer = clients.ExtractContainer;
    private readonly BlobContainerClient _copyContainer = clients.CopyContainer;

    /// <summary>
    /// Uploads a file stream to the primary initialized blob container.
    /// </summary>
    /// <param name="fileName">The name of the file to be uploaded as a blob.</param>
    /// <param name="fileStream">The file data stream to upload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        var blobClient = _primaryContainer.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);
    }

    /// <summary>
    /// Uploads a stream to the extraction blob container and overwrites the blob if it already exists.
    /// </summary>
    /// <param name="blobName">The name of the blob to be created or overwritten.</param>
    /// <param name="stream">The data stream to upload into the blob.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadExtractBlobMetadataAsync(
        string blobName,
        Stream stream,
        CancellationToken ct)
    {
        var blobClient = _extractContainer.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, overwrite: true, ct);
    }

    /// <summary>
    /// Uploads a stream to the copy blob container and overwrites the blob if it already exists.
    /// </summary>
    /// <param name="blobName">The name of the blob to be created or overwritten.</param>
    /// <param name="stream">The data stream to upload into the blob.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadCopyBlobAsync(
        string blobName,
        Stream stream,
        CancellationToken ct)
    {
        var blobClient = _copyContainer.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, overwrite: true, ct);
    }
}
