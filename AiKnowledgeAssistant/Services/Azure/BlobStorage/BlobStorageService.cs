using AiKnowledgeAssistant.Services.Azure.KeyVault;
using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.Azure.BlobStorage;

public sealed class BlobStorageService : IBlobStorageService
{
    private BlobContainerClient? _containerClient;
    private readonly ISecretProvider _secretProvider;

    public BlobStorageService(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public async Task InitializeAsync()
    {
        var connectionString = await _secretProvider.GetSecretValueAsync("Azure--StorageConnectionString")
            ?? throw new InvalidOperationException("Missing secret: Azure--StorageConnectionString");
        var containerName = await _secretProvider.GetSecretValueAsync("Azure--BlobContainerName")
            ?? throw new InvalidOperationException("Missing secret: Azure--BlobContainerName");

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task UploadAsync(string fileName, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(_containerClient, nameof(_containerClient));
        await _containerClient.UploadBlobAsync(fileName, fileStream);
    }
}
