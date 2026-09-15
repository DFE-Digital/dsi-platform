using Dfe.SignIn.Base.Framework.OperationResults;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Strongly-typed errors for Entra email operations.
/// </summary>
public static class EntraEmailErrors
{
    /// <summary>
    /// Indicates that the provided external user ID is invalid (empty).
    /// </summary>
    public static readonly OperationError InvalidUserId = new("Entra.Email.InvalidUserId", "The provided user ID is empty.");

    /// <summary>
    /// Indicates that the provided email address is invalid (empty or whitespace).
    /// </summary>
    public static readonly OperationError InvalidEmailAddress = new("Entra.Email.InvalidEmailAddress", "The provided email address is empty or whitespace.");

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
    public static OperationError UserUpdateFailed(string detail)
        => new(UserUpdateFailedCode, $"Failed to update user email in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that updating the user's MFA email authentication method in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static OperationError MfaAuthenticationMethodFailed(string detail)
        => new(MfaAuthenticationMethodFailedCode, $"Failed to update MFA email authentication method in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating that the user with the specified Entra OID was not found.
    /// </summary>
    /// <param name="userId">The Entra OID of the user.</param>
    /// <returns>The created error.</returns>
    public static OperationError UserNotFound(Guid userId)
        => new("Entra.Email.UserNotFound", $"User with Entra OID '{userId}' was not found.");

    /// <summary>
    /// Creates an error indicating that an unexpected error occurred during Entra email synchronization.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static OperationError Unexpected(string detail)
        => new("Entra.Email.Unexpected", $"An unexpected error occurred during Entra email synchronization: {detail}");
}
