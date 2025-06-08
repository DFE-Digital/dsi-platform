namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// A model that represents a content topic.
/// </summary>
/// <remarks>
///   <para>There is typically a 1:1 mapping between topics and content markdown files.</para> 
/// </remarks>
public sealed record TopicModel()
{
    /// <summary>
    /// Gets the path of the topic.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the metadata of the topic.
    /// </summary>
    public required TopicMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the HTML representation of the topic.
    /// </summary>
    public required string ContentHtml { get; init; }
}
