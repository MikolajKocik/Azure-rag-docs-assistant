using AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;

namespace AiKnowledgeAssistant.Services.OpenAI.Ranker;

public interface IChunkRanker
{
    Task<IReadOnlyList<RankedChunk>> RankAsync(
        string query,
        IReadOnlyList<string> chunks,
        CancellationToken cancellationToken
    );
}