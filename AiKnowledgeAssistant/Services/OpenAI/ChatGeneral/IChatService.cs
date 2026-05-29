namespace AiKnowledgeAssistant.Services.OpenAI.ChatGeneral;

public interface IChatService
{
    Task<string> AskAsync(string request, string context, CancellationToken cancellationToken);
    Task<string> RetrieveDocumentAsync(string request, CancellationToken cancellationToken);
}
