using AiKnowledgeAssistant.Services.BlobStorage;
using AiKnowledgeAssistant.Services.KeyVault;
using AiKnowledgeAssistant.Services.OpenAI;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Search.Documents;
using Microsoft.SemanticKernel.Text;
using System.Text;

namespace AiKnowledgeAssistant.Services;

#pragma warning disable SKEXP0050
public sealed class IngestionService : IIngestionService
{
    private DocumentAnalysisClient _analysisClient;
    private readonly ISecretProvider _secretProvider;
    private readonly IBlobStorageService _blobService;
    private readonly TextEmbeddingService _embeddingService;
    private readonly string _modelId = "prebuilt-document";
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(50, 50);

    public IngestionService(
        ISecretProvider secretProvider,
        IBlobStorageService blobService,
        TextEmbeddingService embeddingService
        )
    {
        _secretProvider = secretProvider;
        _blobService = blobService;
        _embeddingService = embeddingService;       
    }

    private async Task<string> ExtractTextAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        AnalyzeDocumentOperation response = await _analysisClient.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _modelId,
            fileStream,
            null,
            cancellationToken);

        AnalyzeResult doc = response.Value;
        var sb = new StringBuilder();

        foreach(var page in doc.Pages)
        {
            foreach(var line in page.Lines)
            {
                sb.AppendLine(line.Content); 
            }
        }

        return sb.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var endpoint = await _secretProvider.GetSecretValueAsync("Azure--Form-Recognizer-Endpoint", cancellationToken)
            ?? throw new ArgumentException("Azure form recognizer endpoint not found");

        var apiKey =  await _secretProvider.GetSecretValueAsync("Azure--Form-Recognizer-Key", cancellationToken)
            ?? throw new ArgumentException("Azure form recognizer key not found");

        _analysisClient = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }
    
    public async Task<string> ProcessDocumentAsync(
        IFormFile form, 
        CancellationToken cancellationToken
        )
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {    
            using var stream = form.OpenReadStream();
            await _blobService.UploadAsync(form.FileName, stream, cancellationToken);

            stream.Position = 0;
            string extractedText = await ExtractTextAsync(stream, cancellationToken);

            var lines = TextChunker.SplitPlainTextLines(extractedText, maxTokensPerLine: 100);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerParagraph: 1000);

            var embeddingTasks = chunks.Select(chunk => 
                _embeddingService.GetEmbeddingAsync(chunk, cancellationToken));

            float[][] embeddingsArray = await Task.WhenAll(embeddingTasks);
            var embeddings = embeddingsArray.ToList();

            string? searchEndpoint = await _secretProvider.GetSecretValueAsync("Azure--search-endpoint");
            ArgumentException.ThrowIfNullOrEmpty(searchEndpoint, nameof(searchEndpoint));
            
            string? searchKey = await _secretProvider.GetSecretValueAsync("Azure--search-key");
            ArgumentException.ThrowIfNullOrEmpty(searchKey, nameof(searchKey));

            var searchClient = new SearchClient(
                new Uri(searchEndpoint),
                "documents-index",
                new AzureKeyCredential(searchKey)
            );

            var documents = chunks.Select((chunk, idx) => new
            {
                id = Guid.NewGuid().ToString(),
                content = chunk,
                embedding = embeddings[idx]
            });

            await searchClient.UploadDocumentsAsync(documents);
            return extractedText;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
#pragma warning restore SKEXP0050
