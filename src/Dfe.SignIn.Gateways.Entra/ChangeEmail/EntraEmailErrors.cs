using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Strongly-typed errors for Entra email operations.
/// </summary>
public static class EntraEmailErrors
{
    /// <summary>
    /// Indicates that updating the user's primary email in Entra failed.
    /// </summary>
    public const string UserUpdateFailedCode = "Entra.Email.UserUpdateFailed";

    /// <summary>
    /// Indicates that updating the user's MFA email authentication method in Entra failed.
    /// </summary>
    public const string MfaAuthenticationMethodFailedCode = "Entra.Email.MfaAuthenticationMethodFailed";

    /// <summary>
    /// Creates an error indicating that updating the user's email in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error UserUpdateFailed(string detail)
        => new(UserUpdateFailedCode, $"Failed to update user email in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that updating the user's MFA email authentication method in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error MfaAuthenticationMethodFailed(string detail)
        => new(MfaAuthenticationMethodFailedCode, $"Failed to update MFA email authentication method in Entra: {detail}");
}
