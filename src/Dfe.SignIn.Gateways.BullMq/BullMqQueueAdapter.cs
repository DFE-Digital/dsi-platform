using BullMQ;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Thin adapter around a sealed <see cref="Queue"/> instance.
/// </summary>
internal sealed class BullMqQueueAdapter : IBullMqQueue
{
    private readonly Func<string, object, JobsOptions, Task<string?>> addAsync;
    private readonly Func<Task> closeAsync;
    private readonly ILogger<BullMqQueueAdapter> logger;

    /// <summary>
    /// Creates an adapter over a real BullMQ <see cref="Queue"/>.
    /// </summary>
    public BullMqQueueAdapter(Queue queue, ILogger<BullMqQueueAdapter> logger)
        : this(
            async (name, data, options) => (await queue.AddAsync(name, data, options)).Id,
            queue.CloseAsync,
            logger)
    {
    }

    /// <summary>
    /// Creates an adapter with injectable queue operations (for unit tests).
    /// </summary>
    internal BullMqQueueAdapter(
        Func<string, object, JobsOptions, Task<string?>> addAsync,
        Func<Task> closeAsync,
        ILogger<BullMqQueueAdapter> logger)
    {
        this.addAsync = addAsync;
        this.closeAsync = closeAsync;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> AddAsync(string name, object data, JobsOptions options)
    {
        var jobId = await this.addAsync(name, data, options);

        if (string.IsNullOrEmpty(jobId)) {
            this.logger.LogWarning("BullMQ job was enqueued without an id for job name {JobName}", name);
            return string.Empty;
        }

        return jobId;
    }

    /// <inheritdoc/>
    public Task CloseAsync() => this.closeAsync();
}
