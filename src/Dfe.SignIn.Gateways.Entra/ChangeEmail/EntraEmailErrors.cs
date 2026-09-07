using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Strongly-typed errors for Entra email operations.
/// </summary>
public static class EntraEmailErrors
{
    /// <summary>
    /// Defines error codes for Entra email operations.
    /// </summary>
    public static class Codes
    {
        /// <summary>
        /// Indicates that updating the user's email in Entra failed.
        /// </summary>
        public const string UserUpdateFailed = "Entra.Email.UserUpdateFailed";

        /// <summary>
        /// Indicates that updating the user's MFA email authentication method in Entra failed.
        /// </summary>
        public const string MfaAuthenticationMethodFailed = "Entra.Email.MfaAuthenticationMethodFailed";

        /// <summary>
        /// Indicates that the user with the specified Entra OID was not found.
        /// </summary>
        public const string UserNotFound = "Entra.Email.UserNotFound";

        /// <summary>
        /// Indicates that an unexpected error occurred during Entra email synchronization.
        /// </summary>
        public const string Unexpected = "Entra.Email.Unexpected";
    }

    /// <summary>
    /// Creates an error indicating that updating the user's email in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error UserUpdateFailed(string detail)
        => new(Codes.UserUpdateFailed, $"Failed to update user email in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that updating the user's MFA email authentication method in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error MfaAuthenticationMethodFailed(string detail)
        => new(Codes.MfaAuthenticationMethodFailed, $"Failed to update MFA email authentication method in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that the user with the specified Entra OID was not found.
    /// </summary>
    /// <param name="userId">The Entra OID of the user.</param>
    /// <returns>The created error.</returns>
    public static Error UserNotFound(Guid userId)
        => new(Codes.UserNotFound, $"User with Entra OID '{userId}' was not found.");

    /// <summary>
    /// Creates an error indicating that an unexpected error occurred during Entra email synchronization.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error Unexpected(string detail)
        => new(Codes.Unexpected, $"An unexpected error occurred during Entra email synchronization: {detail}");
}
