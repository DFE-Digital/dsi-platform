using Dfe.SignIn.Core.Contracts.Messaging;

namespace Dfe.SignIn.Core.Interfaces.Messaging;

/// <summary>
/// Generic outbound port for publishing integration events.
/// The implementation details (queues, topics, mapping) are handled by the infrastructure adapter.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the configured message broker.
    /// </summary>
    /// <typeparam name="TEvent">The type of the integration event.</typeparam>
    /// <param name="event">The integration event to publish.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;
}
