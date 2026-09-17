using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Matgar.Api.HealthChecks
{
    public sealed class RedisHealthCheck(IServiceProvider serviceProvider) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();

            if (multiplexer is null)
                return HealthCheckResult.Degraded("Redis is not configured");

            try
            {
                var latency = await multiplexer.GetDatabase().PingAsync();

                return multiplexer.IsConnected
                    ? HealthCheckResult.Healthy($"Redis is reachable ({latency.TotalMilliseconds:F0} ms)")
                    : HealthCheckResult.Unhealthy("Redis is disconnected");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
}
