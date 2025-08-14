using AiKnowledgeAssistant.Endpoints;
using AiKnowledgeAssistant.Extensions;
using AiKnowledgeAssistant.Services.Azure.KeyVault;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models;

var builder = WebApplication.CreateBuilder(args);

// Azure Resources
AzureKeyVault.ConfigureKeyVault(builder);
BlobStorageServiceExtension.SetBlobStorage(builder.Services);

// OpenAI Models
GPT_4_Model.ConfigureChatGPT4();
TextEmbeddingServiceExtension.SetTextEmbeddingModel(builder.Services);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

HealthCheck.MapHealthCheckEndpoint(app);

await app.RunAsync();
