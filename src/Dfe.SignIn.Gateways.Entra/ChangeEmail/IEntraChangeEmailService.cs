namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Service for updating user email addresses and MFA methods in Microsoft Entra ID.
/// </summary>
public interface IEntraChangeEmailService
{
    /// <summary>
    /// Changes the primary email address and updates the MFA email authentication method for a user in Microsoft Entra ID.
    /// </summary>
    /// <param name="externalUserId">The external user ID.</param>
    /// <param name="newEmailAddress">The new email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// Primary email may succeed while MFA email-method update fails. Callers that treat
    /// <see cref="EntraEmailErrors.MfaAuthenticationMethodFailedCode"/> as a soft failure
    /// should expect Entra primary mail to already reflect <paramref name="newEmailAddress"/>.
    /// </remarks>
    Task<Base.Framework.OperationResults.OperationResult> ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken = default);
}
