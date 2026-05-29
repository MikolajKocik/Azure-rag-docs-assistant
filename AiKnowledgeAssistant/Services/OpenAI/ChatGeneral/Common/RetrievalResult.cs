using AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;

namespace AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;

public sealed record RetrievalResult(
    string Context, 
    IReadOnlyList<RankedChunk> RankedChunks,
    IReadOnlyList<RankedChunk> SelectedChunks,
    int VectorTopK,
    int FinalTopK,
    long LatencyMs
); 