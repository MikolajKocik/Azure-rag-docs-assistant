namespace AiKnowledgeAssistant.Services.Azure.KeyVault;

public interface ISecretProvider
{
    Task<string?> GetSecretValueAsync(string name, CancellationToken ct = default);
}
