using Dfe.SignIn.Core.Interfaces.Notifications;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// A no-op implementation of <see cref="IUserUpdatedPublisher"/> for presentation use.
/// </summary>
public sealed class StubUserUpdatedPublisher : IUserUpdatedPublisher
{
    /// <inheritdoc/>
    public Task PublishUserUpdatedAsync(Guid userId, string emailAddress, string firstName, string lastName, short status, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
