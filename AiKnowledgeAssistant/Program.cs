using AiKnowledgeAssistant.Endpoints;
using AiKnowledgeAssistant.Extensions;
using AiKnowledgeAssistant.Services.Azure.AppInsights;
using AiKnowledgeAssistant.Services.Azure.KeyVault;
using AiKnowledgeAssistant.Services.AzureOpenAI.FormRecognizer;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models.GPT;

var builder = WebApplication.CreateBuilder(args);

// Azure Resources
AzureKeyVault.ConfigureKeyVault(builder);
ApplicationInsightsService.ConfigureAppInsights(builder);

if (!builder.Environment.IsEnvironment("Testing"))
{
    BlobStorageServiceExtension.SetBlobStorage(builder.Services);
}

// OpenAI models
builder.Services.AddSingleton<IChatService, GPT_4_Model>();
TextEmbeddingServiceExtension.SetTextEmbeddingModel(builder.Services);

builder.Services.AddSingleton<FormRecognizerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();

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

UploadBlob.MapUploadBlobEndpoint(app);
HealthCheck.MapHealthCheckEndpoint(app);
ProcessDocument.MapFormRecognizerEndpoint(app);

await app.RunAsync();

// For HttpClient Blob Storage Tests
public partial class Program { }