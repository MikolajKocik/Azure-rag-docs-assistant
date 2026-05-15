namespace AiKnowledgeAssistant.Services.OpenAI;

public interface ILocalRankerService
{
    float CalculateScore(string query, string document);        
}
