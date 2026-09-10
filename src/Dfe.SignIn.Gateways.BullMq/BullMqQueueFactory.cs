using System.Collections.Concurrent;
using BullMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Factory for creating and managing BullMQ queues.
/// </summary>
public interface IBullMqQueueFactory : IAsyncDisposable
{
    /// <summary>
    /// Gets a BullMQ queue by name. If the queue does not exist, it will be created.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>The BullMQ queue.</returns>
    IBullMqQueue GetQueue(string queueName);

    /// <summary>
    /// Gets the default job options for BullMQ jobs.
    /// </summary>
    /// <returns>The default job options.</returns>
    JobsOptions GetDefaultJobOptions();
}

/// <summary>
/// Implementation of <see cref="IBullMqQueueFactory"/> that manages BullMQ queues.
/// </summary>
/// <param name="options">The BullMQ options.</param>
/// <param name="logger">The logger.</param>
/// <param name="loggerFactory">The logger factory used for queue adapters.</param>
public sealed class BullMqQueueFactory(
    IOptions<BullMqSettings> options,
    ILogger<BullMqQueueFactory> logger,
    ILoggerFactory loggerFactory) : IBullMqQueueFactory
{
    private readonly ConcurrentDictionary<string, Lazy<IBullMqQueue>> queues = new(StringComparer.OrdinalIgnoreCase);
    private readonly BullMqSettings options = options.Value;

    /// <inheritdoc/>
    public IBullMqQueue GetQueue(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var lazyQueue = this.queues.GetOrAdd(queueName, name => new Lazy<IBullMqQueue>(
            () => {
                logger.LogInformation("Initialising BullMQ queue '{QueueName}' on database {DbIndex}", name, this.options.DatabaseIndex);

                var queue = new Queue(name, new QueueOptions {
                    Connection = new ConnectionOptions {
                        ConnectionString = this.options.ConnectionString
                    }
                });

                return new BullMqQueueAdapter(queue, loggerFactory.CreateLogger<BullMqQueueAdapter>());
            },
            LazyThreadSafetyMode.ExecutionAndPublication));

        return lazyQueue.Value;
    }

    /// <inheritdoc/>
    public JobsOptions GetDefaultJobOptions()
    {
        return new JobsOptions {
            RemoveOnComplete = new KeepJobs {
                Age = this.options.RemoveOnCompleteAgeSeconds,
                Count = this.options.RemoveOnCompleteCount
            },
            RemoveOnFail = new KeepJobs {
                Age = this.options.RemoveOnFailAgeSeconds
            }
        };
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (var lazyQueue in this.queues.Values) {
            if (!lazyQueue.IsValueCreated) {
                continue;
            }

            try {
                await lazyQueue.Value.CloseAsync();
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Error closing BullMQ queue connection during shutdown");
            }
        }

        this.queues.Clear();
    }
}
