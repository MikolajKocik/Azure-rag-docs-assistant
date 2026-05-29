using AiKnowledgeAssistant.Configurations;
using AiKnowledgeAssistant.Services.BlobStorage;
using AiKnowledgeAssistant.Services.OpenAI.ChatEmbeddings;
using AiKnowledgeAssistant.Services.OpenAI.ChatGeneral;
using AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;
using AiKnowledgeAssistant.Services.OpenAI.DataIngestion;
using AiKnowledgeAssistant.Services.OpenAI.DataIngestion.Common;
using AiKnowledgeAssistant.Services.OpenAI.Ranker;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights.Extensibility;

namespace AiKnowledgeAssistant.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static async Task ConfigureServicesAsync(this WebApplicationBuilder builder)
        {
            ConfigurationManager cfg = builder.Configuration;
            IServiceCollection services = builder.Services;

            var secretClient = ConfigureKeyVault(builder);

            await ConfigureAzureClientsAsync(services, cfg, secretClient);

            ConfigureAppInsights(builder);
            SetOpenAIServices(builder.Services, builder.Configuration);
        }

        private static void SetOpenAIServices(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<RagOptions>(cfg.GetSection("Rag"));
            services.Configure<AzureOpenAIOptions>(cfg.GetSection("AzureOpenAI"));
            services.Configure<IngestionOptions>(cfg.GetSection("Ingestion"));

            services.AddSingleton<ITextEmbeddingService, TextEmbeddingService>();
            services.AddSingleton<IIngestionService, IngestionService>();
            services.AddSingleton<IChatService, ChatService>();

            // Reranker strategy for evals
            string? rankerType = cfg["Rag:Ranker"];
            ArgumentException.ThrowIfNullOrWhiteSpace(rankerType);

            if (rankerType == "local_reranker")
            {
                services.AddSingleton<IChunkRanker>(_ =>
                {
                    var modelPath = cfg["Ranker:ModelPath"]
                        ?? throw new InvalidOperationException("Ranker model path not found.");
                    var tokenizerPath = cfg["Ranker:TokenizerPath"]
                        ?? throw new InvalidOperationException("Ranker tokenizer path not found.");

                    return new LocalRankerService(modelPath, tokenizerPath);
                });
            }
            else if (rankerType == "none")
            {
                services.AddSingleton<IChunkRanker, NoOpChunkRanker>();
            }
            else
            {
                throw new InvalidOperationException($"Unknown ranker type: {rankerType}");
            }
        }

        private static SecretClient ConfigureKeyVault(WebApplicationBuilder builder)
        {
            string kvUri = builder.Configuration["KEYVAULT_URI"]
                ?? throw new InvalidOperationException("KEYVAULT_URI not found.");

            var secretClient = new SecretClient(
                new Uri(kvUri),
                new DefaultAzureCredential());

            builder.Services.AddSingleton(secretClient);

            return secretClient;
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
        
        private static async Task ConfigureAzureClientsAsync(
        IServiceCollection services,
        IConfiguration cfg,
        SecretClient secretClient)
        {
            string searchEndpoint = (await secretClient.GetSecretAsync("Azure--search-endpoint"))
                .Value.Value;

            string searchKey = (await secretClient.GetSecretAsync("Azure--search-key"))
                .Value.Value;

            string searchIndexName = cfg["AzureSearch:IndexName"] ?? "documents-index";

            services.AddSingleton(new SearchClient(
                new Uri(searchEndpoint),
                searchIndexName,
                new AzureKeyCredential(searchKey)));

            string openAiEndpoint = (await secretClient.GetSecretAsync("Azure--OpenAI--Endpoint"))
                .Value.Value;

            string openAiKey = (await secretClient.GetSecretAsync("Azure--OpenAI--Key"))
                .Value.Value;

            services.AddSingleton(new AzureOpenAIClient(
                new Uri(openAiEndpoint),
                new AzureKeyCredential(openAiKey)));

            string storageConnectionString = (await secretClient.GetSecretAsync("Azure--StorageConnectionString"))
                .Value.Value;

            string primaryContainerName = (await secretClient.GetSecretAsync("Azure--BlobContainerName"))
                .Value.Value;

            string extractContainerName = (await secretClient.GetSecretAsync("Azure--FunctionAppBlobContainerNameExtract"))
                .Value.Value;

            string copyContainerName = (await secretClient.GetSecretAsync("Azure--FunctionAppBlobContainerNameCopy"))
                .Value.Value;

            services.AddSingleton(new BlobStorageClients(
                new BlobContainerClient(storageConnectionString, primaryContainerName),
                new BlobContainerClient(storageConnectionString, extractContainerName),
                new BlobContainerClient(storageConnectionString, copyContainerName)
            ));
        }
    }
}