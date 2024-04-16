using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway;

public static class Probes
{
    private const string LivenessProbe = $"/{LivenessTag}";
    private const string ReadinessProbe = $"/{ReadinessTag}";
    private const string LivenessTag = "alive";
    private const string ReadinessTag = "health";

    public static WebApplication UseProbes(this WebApplication app)
    {
        app.MapHealthChecks(
            ReadinessProbe,
            new HealthCheckOptions
            {
                Predicate = (x) => x.Tags.Contains(ReadinessTag, StringComparer.OrdinalIgnoreCase),
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            }
        );

        app.MapHealthChecks(
            LivenessProbe,
            new HealthCheckOptions
            {
                Predicate = (x) => x.Tags.Contains(LivenessTag, StringComparer.OrdinalIgnoreCase),
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            }
        );

        return app;
    }
}