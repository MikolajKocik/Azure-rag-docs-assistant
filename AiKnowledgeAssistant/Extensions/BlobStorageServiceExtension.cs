using AiKnowledgeAssistant.Services.Azure.BlobStorage;
using AiKnowledgeAssistant.Services.Azure.KeyVault;

namespace AiKnowledgeAssistant.Extensions;

public static class BlobStorageServiceExtension
{
    public static void SetBlobStorage(this IServiceCollection services)
    {
        services.AddSingleton<IBlobStorageService>(provider =>
        {
            var secretProvider = provider.GetRequiredService<ISecretProvider>();
            var blobService = new BlobStorageService(secretProvider);
            blobService.InitializeAsync().GetAwaiter().GetResult();
            return blobService;
        });
    }
}
