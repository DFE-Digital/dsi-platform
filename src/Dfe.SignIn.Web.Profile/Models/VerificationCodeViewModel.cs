using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to input a verification code.
/// </summary>
public sealed class VerificationCodeViewModel
{
    /// <summary>
    /// Gets or sets the new email address of the user.
    /// </summary>
    [ValidateNever]
    public required string NewEmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the user's verification code.
    /// </summary>
    public string? VerificationCodeInput { get; set; }
}
