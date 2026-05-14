namespace AiKnowledgeAssistant.Services.OpenAI
{
    public interface ITextEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken);
    }
}