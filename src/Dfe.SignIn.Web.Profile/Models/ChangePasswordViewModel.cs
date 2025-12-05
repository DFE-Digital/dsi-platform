using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their password.
/// </summary>
public sealed class ChangePasswordViewModel
{
    /// <summary>
    /// Gets or sets the current password of the user.
    /// </summary>
    [DataType(DataType.Password)]
    [MapTo<SelfChangePasswordRequest>(nameof(SelfChangePasswordRequest.CurrentPassword))]
    public string? CurrentPasswordInput { get; set; }

    /// <summary>
    /// Gets or sets the new password of the user.
    /// </summary>
    [DataType(DataType.Password)]
    [MapTo<SelfChangePasswordRequest>(nameof(SelfChangePasswordRequest.NewPassword))]
    public string? NewPasswordInput { get; set; }

    /// <summary>
    /// Gets or sets a confirmation of the user's new password.
    /// </summary>
    [DataType(DataType.Password)]
    [MapTo<SelfChangePasswordRequest>(nameof(SelfChangePasswordRequest.ConfirmNewPassword))]
    public string? ConfirmNewPasswordInput { get; set; }
}
