using AiKnowledgeAssistant.Services.Azure.KeyVault;
using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.Azure.AppFunction;

public sealed class ApplicationFunctionService : IApplicationFunctionService
{
    private readonly ISecretProvider _secretProvider;
    private BlobContainerClient? _extractContainerClient;
    private BlobContainerClient? _copyContainerClient;

    public ApplicationFunctionService(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    private async Task<string> GetStorageAccountConnStringAsync(CancellationToken ct = default)
    {
        var conn = await _secretProvider.GetSecretValueAsync("Azure--FunctionBlobConnStorage")
            ?? throw new ArgumentException("Azure--FunctionBlobConnStorage not found");

        return conn;
    }

    public async Task UploadExtractBlobMetadataAsync(string blobName, Stream stream, CancellationToken ct)
    {
        var conn = await GetStorageAccountConnStringAsync(ct);

        var containerName = await _secretProvider.GetSecretValueAsync("Azure--FunctionAppBlobContainerNameExtract")
            ?? throw new ArgumentException("Azure--FunctionAppBlobContainerNameExtract not found");

        _extractContainerClient ??= new BlobContainerClient(conn, containerName);

        var getBlob = _extractContainerClient.GetBlobClient(blobName);
        await getBlob.UploadAsync(stream, overwrite: true, ct);
    }

    public async Task UploadCopyBlobAsync(string blobName, Stream stream, CancellationToken ct)
    {
        var conn = await GetStorageAccountConnStringAsync(ct);

        var containerName = await _secretProvider.GetSecretValueAsync("Azure--FunctionAppBlobContainerNameCopy")
            ?? throw new ArgumentException("Azure--FunctionAppBlobContainerNameCopy not found");

        _copyContainerClient ??= new BlobContainerClient(conn, containerName);

        var getBlob = _copyContainerClient.GetBlobClient(blobName);
        await getBlob.UploadAsync(stream, overwrite: true, ct);
    }
}
