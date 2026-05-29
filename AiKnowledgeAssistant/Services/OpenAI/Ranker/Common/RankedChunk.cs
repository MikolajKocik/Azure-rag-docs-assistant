namespace AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;

public sealed record RankedChunk(
    string Content,
    float Score,
    string RankerName
);