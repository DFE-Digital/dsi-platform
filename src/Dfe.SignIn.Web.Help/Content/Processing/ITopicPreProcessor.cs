namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// Represents a service that pre-processes the topic markdown content.
/// </summary>
/// <remarks>
///   <para>Some example use cases:</para>
///   <list type="bullet">
///     <item>Replace placeholder variable names with real values.</item>
///   </list>
/// </remarks>
public interface ITopicPreProcessor
{
    /// <summary>
    /// Process the markdown encoded content of a topic.
    /// </summary>
    /// <param name="topicPath">The unique topic path.</param>
    /// <param name="markdown">Markdown encoded content.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The processed markdown encoded content.</para>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<string> ProcessAsync(string topicPath, string markdown, CancellationToken cancellationToken = default);
}
