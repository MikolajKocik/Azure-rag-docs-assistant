using AiKnowledgeAssistant.Endpoints;
using AiKnowledgeAssistant.Extensions;
using AiKnowledgeAssistant.Models;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024; 
});

ModelConfig.ConfigureModelONNX();
await builder.ConfigureServicesAsync();
builder.Services.ConfigureReRanker();

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
HealthCheck.MapHealthCheckEndpoint(app);
ProcessDocument.MapFormRecognizerEndpoint(app);
Ask.MapAskEndpoint(app);

await app.RunAsync();

public partial class Program { }