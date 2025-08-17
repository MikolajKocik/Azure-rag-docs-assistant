using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AiKnowledgeAssistant.Services.AzureOpenAI.Models.GPT;

public sealed class GPT_4_Model : IChatService
{
    private readonly ChatClient _chatClient;

    public GPT_4_Model(IConfiguration config)
    {
        var endpoint = new Uri(config["AZURE_OPENAI_ENDPOINT"]
          ?? throw new ArgumentException("Azure OpenAI endpoint not found"));

        string apiKey = config["AZURE_OPENAI_KEY"]
             ?? throw new ArgumentException("Azure OpenAI api key not found");
        string deploymentName = "gpt-4-chat";

        var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        _chatClient = azureClient.GetChatClient(deploymentName);
    }

    public async Task<string> AskAsync(string prompt)
    {
        var requestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = 4096,
            Temperature = 0.7f,
        };

        List<ChatMessage> messages = new()
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(prompt),
        };

        var response = await _chatClient.CompleteChatAsync(messages, requestOptions);
        return response.Value.Content[0].Text;
    }
}
