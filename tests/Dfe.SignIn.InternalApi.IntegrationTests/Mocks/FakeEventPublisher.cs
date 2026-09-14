using Dfe.SignIn.Core.Contracts.Messaging;
using Dfe.SignIn.Core.Interfaces.Messaging;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeEventPublisher : IEventPublisher
{
    public List<IIntegrationEvent> PublishedEvents { get; } = [];

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        this.PublishedEvents.Add(@event);
        return Task.CompletedTask;
    }

    public IEnumerable<T> GetPublishedEvents<T>() where T : class, IIntegrationEvent
        => this.PublishedEvents.OfType<T>();

    public void Clear()
    {
        this.PublishedEvents.Clear();
    }
}
