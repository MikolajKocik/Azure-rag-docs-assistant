namespace AiKnowledgeAssistant.Services.OpenAI.ChatEmbeddings;

public interface ITextEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken);
}