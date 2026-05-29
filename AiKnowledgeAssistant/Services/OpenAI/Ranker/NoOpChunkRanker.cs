using AiKnowledgeAssistant.Services.OpenAI.Ranker.Common;

namespace AiKnowledgeAssistant.Services.OpenAI.Ranker;

public sealed class NoOpChunkRanker : IChunkRanker
{
    public Task<IReadOnlyList<RankedChunk>> RankAsync(string query, IReadOnlyList<string> chunks, CancellationToken cancellationToken)
    {
        var ranked = chunks
            .Select((chunk, index) => new RankedChunk(
                Content: chunk,
                Score: 1.0f - index * 0.001f,
                RankerName: "none"
            ))
            .ToList();

        return Task.FromResult<IReadOnlyList<RankedChunk>>(ranked);
    }
}