using MervaApi.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MervaApi.Health;

public class DatabaseHealthCheck(MervaDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database unreachable");
    }
}
