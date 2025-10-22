using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the main profile landing page.
/// </summary>
public sealed class HomeViewModel
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [ValidateNever]
    public required string FullName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [ValidateNever]
    public required string EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the pending new email address of the user (if any).
    /// </summary>
    [ValidateNever]
    public string? PendingEmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    [ValidateNever]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets whether the "change password" action is presented.
    /// </summary>
    [ValidateNever]
    public bool ShowChangePasswordAction { get; set; }

    /// <summary>
    /// Gets or sets whether the "change email address" action is presented.
    /// </summary>
    [ValidateNever]
    public bool ShowChangeEmailAction { get; set; }
}
