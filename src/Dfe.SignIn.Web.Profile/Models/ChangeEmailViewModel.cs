namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their email address.
/// </summary>
public sealed class ChangeEmailViewModel
{
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? EmailAddressInput { get; set; }
}
