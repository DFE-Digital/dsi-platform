using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Strongly-typed errors for Entra email operations.
/// </summary>
public static class EntraEmailErrors
{
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
        => new("Entra.Email.UserUpdateFailed", $"Failed to update user email in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that updating the user's MFA email authentication method in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error MfaAuthenticationMethodFailed(string detail)
        => new(MfaAuthenticationMethodFailedCode, $"Failed to update MFA email authentication method in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that the user with the specified Entra OID was not found.
    /// </summary>
    /// <param name="userId">The Entra OID of the user.</param>
    /// <returns>The created error.</returns>
    public static Error UserNotFound(Guid userId)
        => new("Entra.Email.UserNotFound", $"User with Entra OID '{userId}' was not found.");

    /// <summary>
    /// Creates an error indicating that an unexpected error occurred during Entra email synchronization.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error Unexpected(string detail)
        => new("Entra.Email.Unexpected", $"An unexpected error occurred during Entra email synchronization: {detail}");
}
