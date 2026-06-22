using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dfe.SignIn.AppHost;

internal sealed class NodeLogHealthCheck : IHealthCheck
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public NodeLogHealthCheck(ResourceLoggerService loggerService, IResource resource, string pattern)
    {
        _ = this.WatchLogsAsync(loggerService, resource, pattern);
    }

    private async Task WatchLogsAsync(ResourceLoggerService loggerService, IResource resource, string pattern)
    {
        await foreach (var batch in loggerService.WatchAsync(resource)) {
            foreach (var line in batch) {
                if (line.Content.Contains(pattern, StringComparison.OrdinalIgnoreCase)) {
                    this._ready.TrySetResult();
                    return;
                }
            }
        }
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(this._ready.Task.IsCompleted
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Waiting for dev server to start..."));
}
