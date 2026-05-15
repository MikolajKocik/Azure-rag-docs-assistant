using AiKnowledgeAssistant.Services.OpenAI;

namespace AiKnowledgeAssistant.Models;

public static class ModelConfig
{
    public static ILocalRankerService ConfigureModelONNX()
    {
        return new LocalRankerService(
            modelPath: "Models/model.onnx", 
            tokenizerPath: "Models/tokenizer.json"
        );
    }

    public static void ConfigureReRanker(this IServiceCollection services)
    {
        services.AddSingleton<ILocalRankerService, LocalRankerService>();
    }
}
