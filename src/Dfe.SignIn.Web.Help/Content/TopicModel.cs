namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// A model that represents a content topic.
/// </summary>
/// <remarks>
///   <para>There is typically a 1:1 mapping between topics and content markdown files.</para>
/// </remarks>
public sealed record TopicModel
{
    /// <summary>
    /// The path of the topic.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// The metadata of the topic.
    /// </summary>
    public required TopicMetadata Metadata { get; init; }

    /// <summary>
    /// A HTML representation of the topic.
    /// </summary>
    public required string ContentHtml { get; init; }
}
