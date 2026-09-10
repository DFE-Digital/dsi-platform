using BullMQ;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Abstraction over a BullMQ queue for testability.
/// </summary>
public interface IBullMqQueue
{
    /// <summary>
    /// Adds a new job to the queue.
    /// </summary>
    /// <param name="name">The job name.</param>
    /// <param name="data">The job payload.</param>
    /// <param name="options">The job options.</param>
    /// <returns>The enqueued job id.</returns>
    Task<string> AddAsync(string name, object data, JobsOptions options);

    /// <summary>
    /// Closes the queue and its connection (when owned).
    /// </summary>
    Task CloseAsync();
}
