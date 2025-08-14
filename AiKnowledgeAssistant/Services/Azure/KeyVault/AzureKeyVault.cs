using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetEnv;

namespace AiKnowledgeAssistant.Services.Azure.KeyVault;

public static class AzureKeyVault
{
    public static void ConfigureKeyVault(this WebApplicationBuilder builder)
    {
        Env.Load();

        string? kvUri = Environment.GetEnvironmentVariable("KEYVAULT_URI")
            ?? throw new ArgumentNullException("KEYVAULT_URI not found", nameof(kvUri));

        builder.Services.AddSingleton(
            new SecretClient(
            new Uri(kvUri),
            new DefaultAzureCredential()));
    }
}
