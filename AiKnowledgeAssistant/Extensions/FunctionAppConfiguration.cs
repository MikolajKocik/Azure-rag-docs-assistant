using AiKnowledgeAssistant.Services.Azure.AppFunction;

namespace AiKnowledgeAssistant.Extensions
{
    public static class FunctionAppConfiguration
    {
        public static void ConfigureAppFunction(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IApplicationFunctionService, ApplicationFunctionService>();
        }
    }
}
