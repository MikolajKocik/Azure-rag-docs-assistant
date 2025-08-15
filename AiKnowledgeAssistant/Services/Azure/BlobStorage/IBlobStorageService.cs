namespace AiKnowledgeAssistant.Services.Azure.BlobStorage;

public interface IBlobStorageService
{
    Task UploadAsync(string fileName, Stream fileStream);
}
