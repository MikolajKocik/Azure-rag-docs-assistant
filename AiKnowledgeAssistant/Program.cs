using AiKnowledgeAssistant.Endpoints;
using AiKnowledgeAssistant.Services.Azure;

var builder = WebApplication.CreateBuilder(args);

AzureKeyVault.AddAzureKeyVault();
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
