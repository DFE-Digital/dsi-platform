using Dfe.SignIn.Core.Contracts.Messaging;
using Dfe.SignIn.Core.Interfaces.Messaging;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeEventPublisher : IEventPublisher
{
    public List<object> PublishedEvents { get; } = [];

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        if (@event is not null) {
            this.PublishedEvents.Add(@event);
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        this.PublishedEvents.Clear();
    }
}
