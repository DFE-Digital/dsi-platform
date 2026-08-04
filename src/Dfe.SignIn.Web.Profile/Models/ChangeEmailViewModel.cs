namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their email address.
/// </summary>
public sealed class ChangeEmailViewModel
{
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [MapTo<Core.Contracts.Users.InitiateChangeEmailAddressRequest>(nameof(Core.Contracts.Users.InitiateChangeEmailAddressRequest.NewEmailAddress))]
    public string? EmailAddressInput { get; set; }
}
