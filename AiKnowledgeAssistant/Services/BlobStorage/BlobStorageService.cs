using AiKnowledgeAssistant.Services.KeyVault;
using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.BlobStorage;

public sealed class BlobStorageService : IBlobStorageService
{
    private BlobContainerClient? _containerClient;
    private readonly ISecretProvider _secretProvider;

    public BlobStorageService(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    /// <summary>
    /// Retrieves the Azure Function storage account connection string from the key vault.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The retrieved storage account connection string.</returns>
    private async Task<string> GetStorageAccountConnStringAsync(CancellationToken ct = default)
    {
        return await _secretProvider.GetSecretValueAsync("Azure--FunctionBlobConnStorage")
            ?? throw new ArgumentException("Azure--FunctionBlobConnStorage not found");
    }

    /// <summary>
    /// Initialize a blob storage container client
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var connectionString = await _secretProvider.GetSecretValueAsync("Azure--StorageConnectionString", cancellationToken)
            ?? throw new InvalidOperationException("Missing secret: Azure--StorageConnectionString");
        var containerName = await _secretProvider.GetSecretValueAsync("Azure--BlobContainerName", cancellationToken)
            ?? throw new InvalidOperationException("Missing secret: Azure--BlobContainerName");

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    /// <summary>
    /// Uploads a stream to the extraction blob container and overwrites the blob if it already exists.
    /// </summary>
    /// <param name="blobName">The name of the blob to be created or overwritten.</param>
    /// <param name="stream">The data stream to upload into the blob.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadExtractBlobMetadataAsync(string blobName, Stream stream, CancellationToken ct)
    {
        var conn = await GetStorageAccountConnStringAsync(ct);

        var containerName = await _secretProvider.GetSecretValueAsync("Azure--FunctionAppBlobContainerNameExtract")
            ?? throw new ArgumentException("Azure--FunctionAppBlobContainerNameExtract not found");

        _containerClient ??= new BlobContainerClient(conn, containerName);

        var getBlob = _containerClient.GetBlobClient(blobName);
        await getBlob.UploadAsync(stream, overwrite: true, ct);
    }

    /// <summary>
    /// Uploads a stream to the copy blob container and overwrites the blob if it already exists.
    /// </summary>
    /// <param name="blobName">The name of the blob to be created or overwritten.</param>
    /// <param name="stream">The data stream to upload into the blob.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadCopyBlobAsync(string blobName, Stream stream, CancellationToken ct)
    {
        var conn = await GetStorageAccountConnStringAsync(ct);

        var containerName = await _secretProvider.GetSecretValueAsync("Azure--FunctionAppBlobContainerNameCopy")
            ?? throw new ArgumentException("Azure--FunctionAppBlobContainerNameCopy not found");

        _containerClient ??= new BlobContainerClient(conn, containerName);

        var getBlob = _containerClient.GetBlobClient(blobName);
        await getBlob.UploadAsync(stream, overwrite: true, ct);
    }

    /// <summary>
    /// Uploads a file stream to the primary initialized blob container.
    /// </summary>
    /// <param name="fileName">The name of the file to be uploaded as a blob.</param>
    /// <param name="fileStream">The file data stream to upload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public async Task UploadAsync(string fileName, Stream fileStream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_containerClient, nameof(_containerClient));
        await _containerClient.UploadBlobAsync(fileName, fileStream, cancellationToken);
    }
}
