using BullMQ;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Thin adapter around a sealed <see cref="Queue"/> instance.
/// </summary>
internal sealed class BullMqQueueAdapter(Queue queue) : IBullMqQueue
{
    /// <inheritdoc/>
    public async Task<string> AddAsync(string name, object data, JobsOptions? options = null)
    {
        var job = await queue.AddAsync(name, data, options);
        return job.Id ?? string.Empty;
    }

    /// <inheritdoc/>
    public Task CloseAsync() => queue.CloseAsync();
}
