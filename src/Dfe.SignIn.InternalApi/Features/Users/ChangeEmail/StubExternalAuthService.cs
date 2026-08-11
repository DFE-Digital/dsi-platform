using Dfe.SignIn.Core.Interfaces.ExternalAuth;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

//todo: this is just a placeholder for now, we will implement this properly when we have an external auth service to send messages to. For now, we just log the call to this method.

/// <summary>
/// A no-op implementation of <see cref="IExternalAuthService"/> for presentation use.
/// </summary>
public sealed class StubExternalAuthService(ILogger<StubExternalAuthService> logger) : IExternalAuthService
{
    /// <inheritdoc/>
    public Task ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken)
    {
        logger.LogInformation("StubExternalAuthService: ChangeEmailAsync called for user {UserId} with new email {Email}", externalUserId, newEmailAddress);
        return Task.CompletedTask;
    }
}
