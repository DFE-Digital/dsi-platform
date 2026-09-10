using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Messaging;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.BullMq.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Implementation of <see cref="IEventPublisher"/> that publishes events to BullMQ queues.
/// </summary>
/// <param name="queueFactory">The factory instance to create BullMQ queues.</param>
/// <param name="messagingOptions">The messaging options.</param>
/// <param name="logger">The logger instance to use for logging.</param>
/// <remarks>
/// Mid-flight cancellation is not honoured because the BullMQ SDK <c>AddAsync</c> has no cancellation overload.
/// The token is checked before enqueue begins.
/// </remarks>
public sealed class BullMqEventPublisher(
    IBullMqQueueFactory queueFactory,
    IOptions<MessagingSettings> messagingOptions,
    ILogger<BullMqEventPublisher> logger) : IEventPublisher
{
    private readonly MessagingSettings messagingSettings = messagingOptions.Value;

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        ct.ThrowIfCancellationRequested();

        if (!this.messagingSettings.Enabled) {
            logger.LogInformation("Global messaging is disabled. Skipping event {EventId} of type {EventType}", @event.EventId, typeof(TEvent).Name);
            return;
        }

        var (queueName, payload) = MapToLegacyFormat(@event);

        logger.LogInformation("Publishing event {EventId} ({EventType}) to BullMQ queue {QueueName}", @event.EventId, typeof(TEvent).Name, queueName);

        var queue = queueFactory.GetQueue(queueName);
        var jobOptions = queueFactory.GetDefaultJobOptions();

        ct.ThrowIfCancellationRequested();

        try {
            var jobId = await queue.AddAsync(queueName, payload, jobOptions);
            logger.LogInformation("Successfully enqueued job {JobId} for event {EventId}", jobId, @event.EventId);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed to enqueue event {EventId} to Redis queue {QueueName}", @event.EventId, queueName);
            throw;
        }
    }

    private static (string QueueName, object Payload) MapToLegacyFormat(IIntegrationEvent @event)
    {
        return @event switch {
            UserUpdatedEvent uue => ("userupdated_v1", SafeUserPayload.FromDomainEvent(uue)),
            _ => throw new NotSupportedException($"Event type {@event.GetType().Name} is not mapped for BullMQ publishing.")
        };
    }
}
