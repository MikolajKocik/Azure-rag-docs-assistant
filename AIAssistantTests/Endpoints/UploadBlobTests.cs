using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.AI.OpenAI;
using Moq;
using AiKnowledgeAssistant.Services.Azure.BlobStorage;

namespace AIAssistantTests.Endpoints;

public sealed class UploadBlobTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UploadBlobTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Telemetry InMemory config 
                services.RemoveAll<TelemetryClient>();
                services.RemoveAll<ITelemetryChannel>();
                services.AddSingleton(provider =>
                {
                    var config = TelemetryConfiguration.CreateDefault();
                    var inMemoryChannel = new InMemoryChannel();
                    config.TelemetryChannel = inMemoryChannel;
                    config.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
                    return new TelemetryClient(config);
                });

                // AzureOpenAI mock
                services.RemoveAll<AzureOpenAIClient>();
                var mockAzureOpenAICient = new Mock<AzureOpenAIClient>();
                services.AddSingleton(mockAzureOpenAICient.Object);

                services.RemoveAll<IBlobStorageService>();
                services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task Should_Return_Ok_After_File_Uploaded()
    {
        // Arrange
        var fileContent = new MultipartFormDataContent();

        var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test file content"));
        byteArrayContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "formFile",
            FileName = "testfile.txt"
        };
        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        fileContent.Add(byteArrayContent, "formFile", "testfile.txt");

        // Act
        var response = await _client.PostAsync("/upload", fileContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
        // JSON string bcs of TypedResults usage in blob endpoint
        responseContent.Should().Be("\"File uploaded successfully\"");
    }

    [Fact]
    public async Task Should_Return_BadRequest_After_No_File_Provided()
    {
        // Arrange
        var emptyFileContent = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/upload", emptyFileContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
        responseContent.Should().Contain("No file uploaded");
    }

    private sealed class InMemoryBlobStorageService : IBlobStorageService
    {
        public Task UploadAsync(string fileName, Stream fileStream)
        {
            return Task.CompletedTask;
        }
    }
}
