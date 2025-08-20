using AiKnowledgeAssistant.Endpoints;
using AiKnowledgeAssistant.Extensions;
using AiKnowledgeAssistant.Services.Azure.AppInsights;
using AiKnowledgeAssistant.Services.Azure.KeyVault;
using AiKnowledgeAssistant.Services.AzureOpenAI.FormRecognizer;
using AiKnowledgeAssistant.Services.AzureOpenAI.Models.GPT;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024; // 30 MB
});

// Azure Resources
AzureKeyVault.ConfigureKeyVault(builder);
ApplicationInsightsService.ConfigureAppInsights(builder);
builder.ConfigureAppFunction();

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
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Endpoints
UploadBlob.MapUploadBlobEndpoint(app);
HealthCheck.MapHealthCheckEndpoint(app);
ProcessDocument.MapFormRecognizerEndpoint(app);
Ask.MapAskEndpoint(app);
SendToAppFunctions.MapSendBlobToAzureFunctions(app);

await app.RunAsync();

// For HttpClient Blob Storage Tests
public partial class Program { }