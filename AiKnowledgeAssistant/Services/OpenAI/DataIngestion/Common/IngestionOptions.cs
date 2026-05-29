namespace AiKnowledgeAssistant.Services.OpenAI.DataIngestion.Common;

public sealed class IngestionOptions
{
    public string DocumentAnalysisModelId { get; init; } = "prebuilt-document";
    public int MaxTokensPerLine { get; init; } = 100;
    public int MaxTokensPerParagraph { get; init; } = 1000;
    public int MaxConcurrentIngestions { get; init; } = 50;
}