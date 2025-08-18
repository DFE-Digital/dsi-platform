namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// The view model of a topic listing item.
/// </summary>
public sealed class TopicListingItemViewModel
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
