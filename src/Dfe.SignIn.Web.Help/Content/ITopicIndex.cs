namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// Represents a service that provides access to indexed topics.
/// </summary>
public interface ITopicIndex
{
    /// <summary>
    /// Get an enumerable collection of all topics.
    /// </summary>
    /// <returns>
    ///   <para>An unordered enumerable collection of all topics.</para>
    /// </returns>
    IEnumerable<TopicModel> GetAllTopics();

    /// <summary>
    /// Gets a topic by its unique topic path.
    /// </summary>
    /// <param name="topicPath">The unique topic path.</param>
    /// <returns>
    ///   <para>The topic when found; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    TopicModel? GetTopic(string topicPath);

    /// <summary>
    /// Gets the parent topic of the specified topic.
    /// </summary>
    /// <param name="topicPath">The unique topic path.</param>
    /// <returns>
    ///   <para>The parent topic when one exists; otherwise, a value of null indicating
    ///   that the given topic is the root topic.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    TopicModel? GetParentTopic(string topicPath);

    /// <summary>
    /// Gets the list of child topics of the specified topic.
    /// </summary>
    /// <param name="topicPath">The unique topic path.</param>
    /// <returns>
    ///   <para>An unordered enumerable collection of zero-or-more child topics.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    IEnumerable<TopicModel> GetChildTopics(string topicPath);
}
