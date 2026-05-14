namespace AiKnowledgeAssistant.Services.KeyVault;

public interface ISecretProvider
{
    Task<string?> GetSecretValueAsync(string name, CancellationToken ct = default);
}
