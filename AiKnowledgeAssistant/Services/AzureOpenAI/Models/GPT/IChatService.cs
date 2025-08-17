namespace AiKnowledgeAssistant.Services.AzureOpenAI.Models.GPT;

public interface IChatService
{
    Task<string> AskAsync(string prompt);
}
