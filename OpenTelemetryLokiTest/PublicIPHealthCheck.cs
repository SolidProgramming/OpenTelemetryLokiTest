using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OpenTelemetryLokiTest
{
    public class PublicIPHealthCheck(IHttpClientFactory clientFactory) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using HttpClient client = clientFactory.CreateClient();
                string result = await client.GetStringAsync("https://api.ipify.org/", cancellationToken);

                if (string.IsNullOrEmpty(result))
                {
                    IReadOnlyDictionary<string, object> data = new Dictionary<string, object>() { { "response empty", "" } };

                    return new HealthCheckResult(HealthStatus.Healthy, data: data);
                }
                else
                {
                    IReadOnlyDictionary<string, object> data = new Dictionary<string, object>() { { "public ip", result } };

                    return new HealthCheckResult(HealthStatus.Healthy, data: data);
                }
            }
            catch (TaskCanceledException tcex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, exception: tcex);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, exception: ex);
            }
        }
    }
}
