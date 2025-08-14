using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetEnv;

namespace AiKnowledgeAssistant.Services.Azure
{
    public static class AzureKeyVault
    {
        public static void AddAzureKeyVault()
        {
            Env.Load();
            string? kvUri = Environment.GetEnvironmentVariable("KEYVAULT_URI");
            ArgumentException.ThrowIfNullOrEmpty(kvUri);

            var client = new SecretClient(
                new Uri(kvUri), 
                new DefaultAzureCredential());
        }
    }
}
