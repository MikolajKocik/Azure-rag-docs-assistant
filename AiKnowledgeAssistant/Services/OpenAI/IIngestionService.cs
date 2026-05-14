namespace AiKnowledgeAssistant.Services.OpenAI
{
    public interface IIngestionService
    {
        Task<string> ProcessDocumentAsync(IFormFile form, CancellationToken cancellationToken);
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}