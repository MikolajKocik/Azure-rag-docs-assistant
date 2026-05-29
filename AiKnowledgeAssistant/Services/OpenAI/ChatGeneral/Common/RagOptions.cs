namespace AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;

public sealed class RagOptions
{
    public int VectorTopK { get; init; } = 50;
    public int FinalTopK { get; init; } = 3;
}