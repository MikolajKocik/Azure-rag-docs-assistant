using AiKnowledgeAssistant.Requests;
using AiKnowledgeAssistant.Services.OpenAI.ChatGeneral;
using AiKnowledgeAssistant.Services.OpenAI.ChatGeneral.Common;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeAssistant.Endpoints
{
    public static class Ask
    {
        public static void MapAskEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/ask", async Task<Results<Ok<object>, BadRequest<string>, ProblemHttpResult>> (
                [FromBody] ChatRequest request,
                [FromServices] IChatService chatService,
                [FromServices] TelemetryClient telemetryClient,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    RetrievalResult response = await chatService.RetrieveDocumentAsync(request.question, cancellationToken);
                    if (response is null)
                    {
                        return TypedResults.Ok<object>(new { 
                            request.question, 
                            answer = "No documents matching the question." 
                        });
                    }

                    string answer = await chatService.AskAsync(request.question, response.Context, cancellationToken);          

                    telemetryClient.TrackEvent("QuestionAnswered", new Dictionary<string, string>
                    {
                        ["Question"] = request.question,
                        ["AnswerLength"] = answer.Length.ToString()
                    });

                    return TypedResults.Ok<object>(new
                    {
                        request.question,
                        answer
                    });
                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);

                    return TypedResults.Problem(
                        detail: $"Error occurred while answering question: {ex.Message}",
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Internal error"
                    );
                }
            })
            .WithName("AskQuestion")
            .WithTags("QnA")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();
        }
    }
}
