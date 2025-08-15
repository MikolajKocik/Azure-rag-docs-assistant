using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace AiKnowledgeAssistant.Services.Azure.AppInsights
{
    public sealed class CloudRoleNameInitializer : ITelemetryInitializer
    {
        private readonly string _roleName;
        private readonly string _roleInstance;

        public CloudRoleNameInitializer(string RoleName, string roleInstance)
        {
            _roleName = RoleName;
            _roleInstance = roleInstance;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = _roleName;
            }

            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleInstance))
            {
                telemetry.Context.Cloud.RoleInstance = _roleInstance;
            }
        }
    }
}
