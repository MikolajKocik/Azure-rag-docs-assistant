using Azure.Security.KeyVault.Secrets;
using AiKnowledgeAssistant.Services.Azure.BlobStorage;

namespace AiKnowledgeAssistant.Services.Azure.KeyVault;

internal sealed class KeyVaultSecretProvider : ISecretProvider
{
    private readonly SecretClient _client;
    public KeyVaultSecretProvider(SecretClient client) => _client = client;

    public async Task<string?> GetSecretValueAsync(string name, CancellationToken ct = default)
    {
        var response = await _client.GetSecretAsync(name, cancellationToken: ct);
        return response.Value.Value;
    }
}
