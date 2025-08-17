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

/// <summary>
/// Provides integration tests for the file upload functionality of the application.
/// </summary>
/// <remarks>This test class is designed to verify the behavior of the file upload endpoint in various scenarios,
/// including successful file uploads and error handling when no file is provided. It uses a custom <see
/// cref="WebApplicationFactory{TEntryPoint}"/> to configure the test environment with in-memory services and mocks for
/// dependencies such as telemetry and blob storage.</remarks>
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

    /// <summary>
    /// Verifies that the file upload endpoint returns an HTTP 200 OK status code after a file is successfully uploaded.
    /// </summary>
    /// <remarks>This test simulates a file upload by sending a multipart form-data request containing a text
    /// file.  It asserts that the response status code is <see cref="HttpStatusCode.OK"/> and that the response content
    /// matches the expected success message.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Verifies that the API returns a <see cref="HttpStatusCode.BadRequest"/> response when no file is provided
    /// in a multipart form-data upload request.
    /// </summary>
    /// <remarks>This test ensures that the server correctly handles invalid upload requests by returning
    /// a 400 Bad Request status code and an appropriate error message indicating that no file was
    /// uploaded.</remarks>
    /// <returns></returns>
    [Fact]
    public async Task Should_Return_BadRequest_After_No_File_Provided()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(string.Empty), "dummy");

        // Act
        var response = await _client.PostAsync("/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
        responseContent.Should().Contain("No file uploaded");
    }

    /// <summary>
    /// Provides an in-memory implementation of the <see cref="IBlobStorageService"/> interface for storing and managing
    /// blobs.
    /// </summary>
    /// <remarks>This implementation is intended for testing or development purposes and does not persist data
    /// beyond the application's lifetime.</remarks>
    private sealed class InMemoryBlobStorageService : IBlobStorageService
    {
        public Task UploadAsync(string fileName, Stream fileStream)
        {
            return Task.CompletedTask;
        }
    }
}
