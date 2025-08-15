using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBlobStorageService));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task Should_Return_File_After_Upload()
    {
        // Arrange
        var fileBytes = Encoding.UTF8.GetBytes("Test file content");
        var fileContent = new MultipartFormDataContent();

        var byteArrayContent = new ByteArrayContent(fileBytes);
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
        responseContent.Should().Be("File uploaded successfully");
    }

    private sealed class InMemoryBlobStorageService : IBlobStorageService
    {
        public Task UploadAsync(string fileName, Stream fileStream)
        {
            return Task.CompletedTask;
        }
    }
}
