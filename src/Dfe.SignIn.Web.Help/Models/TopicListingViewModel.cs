namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// The view model of a listing of topics.
/// </summary>
public sealed class TopicListingViewModel
{
    /// <summary>
    /// Heading of the card section.
    /// </summary>
    public required string Heading { get; set; }

    /// <summary>
    /// The ordered enumerable collection of topic listing items.
    /// </summary>
    public required IEnumerable<TopicListingItemViewModel> Topics { get; set; }
}
