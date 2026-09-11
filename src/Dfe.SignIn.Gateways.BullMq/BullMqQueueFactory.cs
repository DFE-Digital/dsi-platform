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
public sealed class BullMqQueueFactory : IBullMqQueueFactory
{
    private readonly ConcurrentDictionary<string, Lazy<IBullMqQueue>> queues = new(StringComparer.OrdinalIgnoreCase);
    private readonly BullMqSettings options;
    private readonly ILogger<BullMqQueueFactory> logger;
    private readonly Func<string, IBullMqQueue> createQueue;

    /// <summary>
    /// Creates a factory that materialises real BullMQ queues.
    /// </summary>
    /// <param name="options">The BullMQ options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory used for queue adapters.</param>
    public BullMqQueueFactory(
        IOptions<BullMqSettings> options,
        ILogger<BullMqQueueFactory> logger,
        ILoggerFactory loggerFactory)
        : this(options, logger, name => CreateQueueAdapter(name, options.Value, loggerFactory))
    {
    }

    /// <summary>
    /// Creates a factory with an injectable queue creator (for unit tests).
    /// </summary>
    internal BullMqQueueFactory(
        IOptions<BullMqSettings> options,
        ILogger<BullMqQueueFactory> logger,
        Func<string, IBullMqQueue> createQueue)
    {
        this.options = options.Value;
        this.logger = logger;
        this.createQueue = createQueue;
    }

    /// <inheritdoc/>
    public IBullMqQueue GetQueue(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var lazyQueue = this.queues.GetOrAdd(queueName, name => new Lazy<IBullMqQueue>(
            () => {
                this.logger.LogInformation(
                    "Initialising BullMQ queue '{QueueName}' on database {DbIndex}",
                    name,
                    this.options.DatabaseIndex);

                return this.createQueue(name);
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
                this.logger.LogWarning(ex, "Error closing BullMQ queue connection during shutdown");
            }
        }

        this.queues.Clear();
    }

    private static IBullMqQueue CreateQueueAdapter(
        string name,
        BullMqSettings settings,
        ILoggerFactory loggerFactory)
    {
        var queue = new Queue(name, new QueueOptions {
            Connection = new ConnectionOptions {
                ConnectionString = settings.ConnectionString
            }
        });

        return new BullMqQueueAdapter(queue, loggerFactory.CreateLogger<BullMqQueueAdapter>());
    }
}
