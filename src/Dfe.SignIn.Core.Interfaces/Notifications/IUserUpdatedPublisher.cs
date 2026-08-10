namespace Dfe.SignIn.Core.Interfaces.Notifications;

/// <summary>
/// Service to publish notifications to downstream applications when a user's details are updated.
/// </summary>
public interface IUserUpdatedPublisher
{
    /// <summary>
    /// Publishes a user updated event.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="emailAddress">The user's email address.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="status">The user's status code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishUserUpdatedAsync(Guid userId, string emailAddress, string firstName, string lastName, short status, CancellationToken cancellationToken);
}
