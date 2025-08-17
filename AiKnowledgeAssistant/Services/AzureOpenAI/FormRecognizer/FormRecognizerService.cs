using AiKnowledgeAssistant.Services.Azure.KeyVault;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text;

namespace AiKnowledgeAssistant.Services.AzureOpenAI.FormRecognizer;

public sealed class FormRecognizerService
{
    private readonly DocumentAnalysisClient _analysisClient;
    private readonly string _modelId = "prebuilt-document";
    private readonly ISecretProvider _secretClient;

    public FormRecognizerService(
        IConfiguration configuration,
        ISecretProvider secretClient
        )
    {
        _secretClient = secretClient;

        var endpoint = _secretClient.GetSecretValueAsync("Azure--Form-Recognizer-Endpoint").GetAwaiter().GetResult()
            ?? throw new ArgumentException("Azure form recognizer endpoint not found");

        var apiKey =  _secretClient.GetSecretValueAsync("Azure--Form-Recognizer-Key").GetAwaiter().GetResult()
            ?? throw new ArgumentException("Azure form recognizer key not found");

        _analysisClient = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    public async Task<string> ExtractTextAsync(Stream fileStream)
    {
        AnalyzeDocumentOperation response = await _analysisClient.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _modelId,
            fileStream);

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
}
