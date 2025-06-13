namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// Represents a service that processes a topic.
/// </summary>
public interface ITopicProcessor
{
    /// <summary>
    /// Process a topic.
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The processed topic.</para>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<TopicModel> ProcessAsync(TopicModel topic, CancellationToken cancellationToken = default);
}
