namespace Dfe.SignIn.WebFramework.Models;

/// <summary>
/// The view model of a navigation card.
/// </summary>
public sealed class CardViewModel
{
    /// <summary>
    /// Title text of card.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Hypertext link to the associated resource.
    /// </summary>
    public required Uri Href { get; set; }

    /// <summary>
    /// Optional, summary text for card.
    /// </summary>
    public string? Summary { get; set; }
}
