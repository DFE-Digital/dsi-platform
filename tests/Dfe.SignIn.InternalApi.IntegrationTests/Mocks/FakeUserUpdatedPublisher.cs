using Dfe.SignIn.Core.Interfaces.Notifications;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeUserUpdatedPublisher : IUserUpdatedPublisher
{
    public List<(Guid UserId, string EmailAddress, string FirstName, string LastName, short Status)> PublishedEvents { get; } = [];

    public Task PublishUserUpdatedAsync(Guid userId, string emailAddress, string firstName, string lastName, short status, CancellationToken cancellationToken)
    {
        PublishedEvents.Add((userId, emailAddress, firstName, lastName, status));
        return Task.CompletedTask;
    }

    public void Clear()
    {
        PublishedEvents.Clear();
    }
}
