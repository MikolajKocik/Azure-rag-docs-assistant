using Azure;
using Azure.AI.OpenAI;
using DotNetEnv;
using OpenAI.Chat;

namespace AiKnowledgeAssistant.Services.AzureOpenAI.Models;

public static class GPT_4_Model
{
    public static void ConfigureChatGPT4()
    {
        Env.Load();

        var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new ArgumentException("Azure OpenAI endpoint not found"));

        string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
             ?? throw new ArgumentException("Azure OpenAI api key not found");
        string deploymentName = "gpt-4-chat";

        var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        ChatClient chatClient = azureClient.GetChatClient(deploymentName);

        var requestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = 4096,
            Temperature = 1.0f,
            TopP = 1.0f
        };

        List<ChatMessage> messages = new()
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("I am going to Paris, what should I see?"),
        };

        var response = chatClient.CompleteChat(messages, requestOptions);
        Console.WriteLine(response.Value.Content[0].Text);
    }
}
