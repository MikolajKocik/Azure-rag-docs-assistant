using Microsoft.ApplicationInsights.Extensibility;

namespace AiKnowledgeAssistant.Services.Azure.AppInsights
{
    public static class ApplicationInsightsService
    {
        public static void ConfigureAppInsights(this WebApplicationBuilder builder)
        {
            string? appConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]; 

            builder.Logging.AddApplicationInsights(
                configureTelemetryConfiguration: (cfg) =>
            {
                cfg.ConnectionString = appConn;
            }, 
            configureApplicationInsightsLoggerOptions: (options) => { }
            );

            builder.Services.Configure<TelemetryConfiguration>(cfg =>
            {
                cfg.TelemetryInitializers.Add(
                    new CloudRoleNameInitializer(
                        "AI Knowledge Assistant API",
                        Environment.MachineName));
            });
        }
    }
}
