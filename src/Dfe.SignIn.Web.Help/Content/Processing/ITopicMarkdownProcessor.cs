namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// Represents a service that processes markdown content into topic models.
/// </summary>
public interface ITopicMarkdownProcessor
{
    /// <summary>
    /// Process the given markdown encoded content.
    /// </summary>
    /// <param name="topicPath">Path of the topic.</param>
    /// <param name="markdown">Markdown encoded content.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>A model representing the processed topic.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="markdown"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="topicPath"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<TopicModel> ProcessMarkdownAsync(string topicPath, string markdown, CancellationToken cancellationToken = default);
}
