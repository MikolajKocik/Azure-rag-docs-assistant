using AiKnowledgeAssistant.Services.AzureOpenAI.Models;

namespace AiKnowledgeAssistant.Extensions
{
    public static class TextEmbeddingServiceExtension
    {
        public static void SetTextEmbeddingModel(this IServiceCollection services)
        {
            services.AddSingleton(async provider =>
            {
                var embeddingService = provider.GetRequiredService<TextEmbeddingService>();
                var vector = await embeddingService.GetEmbeddingAsync("This is a test string");
                Console.WriteLine($"Embedding length: {vector.Length}");
            });
        }
    }
}
