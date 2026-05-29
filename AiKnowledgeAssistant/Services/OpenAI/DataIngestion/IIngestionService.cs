namespace AiKnowledgeAssistant.Services.OpenAI.DataIngestion;

public interface IIngestionService
{
    Task<string> ProcessDocumentAsync(IFormFile form, CancellationToken cancellationToken);
}
