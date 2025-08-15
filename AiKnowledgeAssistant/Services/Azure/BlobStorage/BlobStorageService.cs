using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.Azure.BlobStorage;

public sealed class BlobStorageService : IBlobStorageService
{
    private BlobContainerClient? _containerClient;
    private readonly SecretClient _secretClient;

    public BlobStorageService(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public async Task InitializeAsync()
    {
        var connectionString = (await _secretClient.GetSecretAsync("Azure--StorageConnectionString"))
            .Value.Value;

        var containerName = (await _secretClient.GetSecretAsync("Azure--BlobContainerName"))
            .Value.Value;

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task UploadAsync(string fileName, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(_containerClient, nameof(_containerClient));
        await _containerClient.UploadBlobAsync(fileName, fileStream);
    }
}
