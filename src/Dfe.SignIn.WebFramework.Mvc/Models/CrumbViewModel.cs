namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// The view model for a breadcrumb.
/// </summary>
public sealed class CrumbViewModel
{
    /// <summary>
    /// The text to show on the breadcrumb.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// The hypertext reference.
    /// </summary>
    public required Uri Href { get; set; }
}
