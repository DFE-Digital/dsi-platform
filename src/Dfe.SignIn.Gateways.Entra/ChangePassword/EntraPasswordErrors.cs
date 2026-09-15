using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Gateways.Entra.ChangePassword;

/// <summary>
/// Domain errors relating to changing passwords in Entra ID.
/// </summary>
public static class EntraPasswordErrors
{
    /// <summary>
    /// The error code when the provided current password is incorrect.
    /// </summary>
    public const string InvalidCurrentPasswordCode = "Entra.Password.InvalidCurrent";
    
    /// <summary>
    /// The error code when the new password violates policy or is breached.
    /// </summary>
    public const string PasswordPolicyViolationCode = "Entra.Password.PolicyViolation";

    /// <summary>
    /// Gets the error when the provided current password is incorrect.
    /// </summary>
    public static readonly Error InvalidCurrentPassword = new(
        InvalidCurrentPasswordCode, "Please enter your current password");

    /// <summary>
    /// Gets the error when the new password violates policy or is breached.
    /// </summary>
    public static Error PasswordPolicyViolation(string message) => new(
        PasswordPolicyViolationCode, message);

    /// <summary>
    /// Gets an unexpected error.
    /// </summary>
    public static Error Unexpected(string message) => new(
        "Entra.Password.Unexpected", message);
}
