using Dfe.SignIn.Core.Interfaces.ExternalAuth;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// A no-op implementation of <see cref="IExternalAuthService"/> for presentation use.
/// </summary>
public sealed class StubExternalAuthService : IExternalAuthService
{
    /// <inheritdoc/>
    public Task ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
