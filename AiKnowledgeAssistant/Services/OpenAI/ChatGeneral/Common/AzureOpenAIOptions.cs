namespace AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;

public sealed class AzureOpenAIOptions
{
    public string ChatDeploymentName { get; init; } = "gpt-4-chat";
    public string EmbeddingDeploymentName { get; init; } = "text-embedding-ada-002";
}