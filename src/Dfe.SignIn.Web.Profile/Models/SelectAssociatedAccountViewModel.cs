namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that prompts the user to select their external account.
/// </summary>
public sealed class SelectAssociatedAccountViewModel
{
    /// <summary>
    /// Gets or sets the URI where the user will be redirected to upon selecting
    /// their external account.
    /// </summary>
    public string? RedirectUri { get; set; }
}
