namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;

/// <summary>
/// Represents a request to change the password of a user.
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Gets or sets the current password of the user.
    /// </summary>
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// Gets or sets the new password that the user wants to set.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Gets or sets the confirmation of the new password to ensure it matches the new password.
    /// </summary>
    public required string ConfirmNewPassword { get; init; }
}
