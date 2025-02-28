namespace Dfe.SignIn.SelectOrganisation.Web.Models;

/// <summary>
/// View model for an invalid session error page.
/// </summary>
public sealed class InvalidSessionViewModel
{
    /// <summary>
    /// Gets the URL that the user can return to.
    /// </summary>
    public required Uri ReturnUrl { get; init; }
}
