namespace Dfe.SignIn.Core.Interfaces.ExternalAuth;

/// <summary>
/// Service to synchronize changes to external authentication provider (Entra ID).
/// </summary>
public interface IExternalAuthService
{
    /// <summary>
    /// Propagates the email address change to the external provider.
    /// </summary>
    /// <param name="externalUserId">The unique identifier of the user on the external provider (Entra OID).</param>
    /// <param name="newEmailAddress">The user's new email address.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Dfe.SignIn.Core.Contracts.Users.FailedToUpdateAuthenticationMethodException">
    /// Thrown when Entra MFA authentication method update fails.
    /// </exception>
    Task ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken);
}
