namespace AiKnowledgeAssistant.Endpoints
{
    public static class HealthCheck
    {
        public static void MapHealthCheckEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () => Results.Ok())
                .WithName("HealthCheck")
                .WithSummary("Health Check Endpoint")
                .WithDescription("This endpoint is used to check the health of the application.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithTags("Health");
        }
    }
}
