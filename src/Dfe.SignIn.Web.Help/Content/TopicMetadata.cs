namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// A model that represents the metadata of a content topic.
/// </summary>
/// <seealso cref="TopicModel"/>
public sealed record TopicMetadata()
{
    /// <summary>
    /// Gets the optional caption of the topic.
    /// </summary>
    public string? Caption { get; init; }

    /// <summary>
    /// Gets the title of the topic.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets a shorter title that can be used for navigation link text.
    /// </summary>
    /// <remarks>
    ///   <para>Assumes the value of <see cref="Title"/> when not specified.</para>
    /// </remarks>
    public required string NavigationTitle { get; init; }

    /// <summary>
    /// Gets a brief summary of the topic.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Gets the ordered enumerable collection of topic listings.
    /// </summary>
    public IEnumerable<TopicListing> Topics { get; init; } = [];
}

/// <summary>
/// A model that represents a listing of topics.
/// </summary>
public sealed record TopicListing()
{
    /// <summary>
    /// Gets the heading of the topic listing.
    /// </summary>
    public string Heading { get; set; } = "Topics in this section";

    /// <summary>
    /// Gets the list of topic paths.
    /// </summary>
    /// <remarks>
    ///   <para>Topic paths are always absolute; for example:</para>
    ///   <list type="bullet">
    ///     <item>"/"</item>
    ///     <item>"/my-account/multifactor-authentication"</item>
    ///   </list>
    /// </remarks>
    public IEnumerable<string> Paths { get; set; } = [];
}
