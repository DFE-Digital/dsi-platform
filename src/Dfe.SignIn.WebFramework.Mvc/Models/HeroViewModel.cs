namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// The view model for the hero of a landing page.
/// </summary>
public sealed class HeroViewModel
{
    /// <summary>
    /// The hero heading text.
    /// </summary>
    public required string Heading { get; set; }

    /// <summary>
    /// Optional, additional text to show within the hero banner.
    /// </summary>
    public string? Text { get; set; }
}
