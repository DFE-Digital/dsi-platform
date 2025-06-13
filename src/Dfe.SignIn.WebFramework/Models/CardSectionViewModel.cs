namespace Dfe.SignIn.WebFramework.Models;

/// <summary>
/// The view model of a section of cards.
/// </summary>
public sealed class CardSectionViewModel
{
    /// <summary>
    /// Heading of the card section.
    /// </summary>
    public required string Heading { get; set; }

    /// <summary>
    /// The ordered enumerable collection of cards.
    /// </summary>
    public required IEnumerable<CardViewModel> Cards { get; set; }
}
