namespace Dfe.SignIn.WebFramework.Models;

/// <summary>
/// The view model of a navigation item.
/// </summary>
public sealed class NavigationItemViewModel
{
    /// <summary>
    /// The text to show on the navigation item.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// The hypertext reference.
    /// </summary>
    public required Uri Href { get; set; }

    /// <summary>
    /// A value indicating if the navigation item is active.
    /// </summary>
    public bool IsActive { get; set; } = false;
}
