using AiKnowledgeAssistant.Services.Azure.BlobStorage;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

namespace AiKnowledgeAssistant.Extensions;

public static class BlobStorageServiceExtension
{
    public static void SetBlobStorage(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var secretClient = provider.GetRequiredService<SecretClient>();
            var blobService = new BlobStorageService(secretClient);

            blobService.InitializeAsync().GetAwaiter().GetResult();

            return blobService;
        });
    }
}
