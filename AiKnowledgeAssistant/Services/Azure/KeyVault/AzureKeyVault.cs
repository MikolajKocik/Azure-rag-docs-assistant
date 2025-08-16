using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AiKnowledgeAssistant.Services.Azure.KeyVault;

public static class AzureKeyVault
{
    public static void ConfigureKeyVault(this WebApplicationBuilder builder)
    {
        string? kvUri = builder.Configuration["KEYVAULT_URI"]
            ?? throw new ArgumentNullException("KEYVAULT_URI not found", nameof(kvUri));

        var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
        builder.Services.AddSingleton(secretClient);
        builder.Services.AddSingleton<ISecretProvider>(new KeyVaultSecretProvider(secretClient));
    }
}
