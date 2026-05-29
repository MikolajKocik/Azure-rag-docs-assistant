using AiKnowledgeAssistant.Services.BlobStorage;
using AiKnowledgeAssistant.Services.OpenAI.ChatEmbeddings;
using AiKnowledgeAssistant.Services.OpenAI.DataIngestion.Common;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using System.Text;

namespace AiKnowledgeAssistant.Services.OpenAI.DataIngestion;

#pragma warning disable SKEXP0050
public sealed class IngestionService(
    DocumentAnalysisClient analysisClient,
    SearchClient searchClient,
    IBlobStorageService blobService,
    ITextEmbeddingService embeddingService,
    IOptions<IngestionOptions> options) : IIngestionService
{
    private readonly DocumentAnalysisClient _analysisClient = analysisClient;
    private readonly SearchClient _searchClient = searchClient;
    private readonly IngestionOptions _options = options.Value;

    private readonly IBlobStorageService _blobService = blobService;
    private readonly ITextEmbeddingService _embeddingService = embeddingService;
    
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(50, 50);

    private async Task<string> ExtractTextAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        AnalyzeDocumentOperation response = await _analysisClient.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _options.DocumentAnalysisModelId,
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
            string extractedText;
            
            string extension = Path.GetExtension(form.FileName).ToLowerInvariant();

            if (extension is ".md" or ".txt")
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                extractedText = await reader.ReadToEndAsync(cancellationToken);
            }
            else
            {
                extractedText = await ExtractTextAsync(stream, cancellationToken);
            }

            List<string> lines = TextChunker.SplitPlainTextLines(
                extractedText, 
                maxTokensPerLine: _options.MaxTokensPerLine
            );
            List<string> chunks = TextChunker.SplitPlainTextParagraphs(
                lines, 
                maxTokensPerParagraph: _options.MaxTokensPerParagraph
            );

            IEnumerable<Task<float[]>> embeddingTasks = chunks.Select(chunk => 
                _embeddingService.GetEmbeddingAsync(chunk, cancellationToken));

            float[][] embeddingsArray = await Task.WhenAll(embeddingTasks);
            var embeddings = embeddingsArray.ToList();

            var documents = chunks.Select((chunk, idx) => new
            {
                id = Guid.NewGuid().ToString(),
                content = chunk,
                embedding = embeddings[idx]
            });

            await _searchClient.UploadDocumentsAsync(documents, cancellationToken: cancellationToken);
            return extractedText;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
#pragma warning restore SKEXP0050
