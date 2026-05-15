using AiKnowledgeAssistant.Configurations;
using AiKnowledgeAssistant.Services;
using AiKnowledgeAssistant.Services.BlobStorage;
using AiKnowledgeAssistant.Services.KeyVault;
using AiKnowledgeAssistant.Services.OpenAI;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Extensibility;

namespace AiKnowledgeAssistant.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            ConfigureKeyVault(builder);
            ConfigureAppInsights(builder);
            SetBlobStorage(builder.Services);
            SetOpenAIServices(builder.Services);
        }

        private static void SetBlobStorage(this IServiceCollection services)
        {
            services.AddSingleton<IBlobStorageService>(provider =>
            {
                var secretProvider = provider.GetRequiredService<ISecretProvider>();
                var blobService = new BlobStorageService(secretProvider);
                return blobService;
            });
        }

        private static void SetOpenAIServices(this IServiceCollection services)
        {
            services.AddSingleton<ITextEmbeddingService, TextEmbeddingService>();
            services.AddSingleton<IIngestionService, IngestionService>();
            services.AddSingleton<IChatService, ChatService>();
        }

        private static void ConfigureKeyVault(this WebApplicationBuilder builder)
        {
            string? kvUri = builder.Configuration["KEYVAULT_URI"]
                ?? throw new ArgumentNullException("KEYVAULT_URI not found", nameof(kvUri));

            var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            builder.Services.AddSingleton(secretClient);
            builder.Services.AddSingleton<ISecretProvider>(new KeyVaultSecretProvider(secretClient));
        }

        private static void ConfigureAppInsights(this WebApplicationBuilder builder)
        {
            string? appConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]; 

            builder.Logging.AddApplicationInsights(
                configureTelemetryConfiguration: (cfg) =>
            {
                cfg.ConnectionString = appConn;
            }, 
            configureApplicationInsightsLoggerOptions: (options) => { }
            );

            builder.Services.Configure<TelemetryConfiguration>(cfg =>
            {
                cfg.TelemetryInitializers.Add(
                    new CloudRoleNameInitializer(
                        "AI Knowledge Assistant API",
                        Environment.MachineName));
            });
        }
    }
}