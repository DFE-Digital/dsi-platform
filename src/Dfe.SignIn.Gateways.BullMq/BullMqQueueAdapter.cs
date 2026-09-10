using BullMQ;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Thin adapter around a sealed <see cref="Queue"/> instance.
/// </summary>
internal sealed class BullMqQueueAdapter(
    Queue queue,
    ILogger<BullMqQueueAdapter> logger) : IBullMqQueue
{
    /// <inheritdoc/>
    public async Task<string> AddAsync(string name, object data, JobsOptions options)
    {
        var job = await queue.AddAsync(name, data, options);

        if (string.IsNullOrEmpty(job.Id)) {
            logger.LogWarning("BullMQ job was enqueued without an id for job name {JobName}", name);
            return string.Empty;
        }

        return job.Id;
    }

    /// <inheritdoc/>
    public Task CloseAsync() => queue.CloseAsync();
}
