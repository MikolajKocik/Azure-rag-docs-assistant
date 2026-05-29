using Azure.Storage.Blobs;

namespace AiKnowledgeAssistant.Services.BlobStorage;
public sealed class BlobStorageClients
{
    public BlobStorageClients(
        BlobContainerClient primaryContainer,
        BlobContainerClient extractContainer,
        BlobContainerClient copyContainer)
    {
        PrimaryContainer = primaryContainer;
        ExtractContainer = extractContainer;
        CopyContainer = copyContainer;
    }

    public BlobContainerClient PrimaryContainer { get; }
    public BlobContainerClient ExtractContainer { get; }
    public BlobContainerClient CopyContainer { get; }
}