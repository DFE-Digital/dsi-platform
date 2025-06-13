using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Web.Help.Content;

public static class TopicIndexExtensions
{
    /// <summary>
    /// Gets a required topic by its unique topic path.
    /// </summary>
    /// <param name="topicPath">The unique topic path.</param>
    /// <returns>
    ///   <para>The topic when found; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicIndex"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///   <para>If topic does not exist at path <paramref name="topicPath"/>.</para>
    /// </exception>
    public static TopicModel GetRequiredTopic(this ITopicIndex topicIndex, string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicIndex, nameof(topicIndex));

        return topicIndex.GetTopic(topicPath)
            ?? throw new KeyNotFoundException($"Topic is missing '{topicPath}'.");
    }

    /// <summary>
    /// Gets the parent topic of the specified topic.
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <returns>
    ///   <para>The parent topic when one exists; otherwise, a value of null indicating
    ///   that the given topic is the root topic.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicIndex"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topic"/> is null.</para>
    /// </exception>
    public static TopicModel? GetParentTopic(this ITopicIndex topicIndex, TopicModel topic)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicIndex, nameof(topicIndex));
        ExceptionHelpers.ThrowIfArgumentNull(topic, nameof(topic));

        return topicIndex.GetParentTopic(topic.Path);
    }

    /// <summary>
    /// Gets the collection of child topics of the specified topic.
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <returns>
    ///   <para>An unordered enumerable collection of zero-or-more child topics.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicIndex"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topic"/> is null.</para>
    /// </exception>
    public static IEnumerable<TopicModel> GetChildTopics(this ITopicIndex topicIndex, TopicModel topic)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicIndex, nameof(topicIndex));
        ExceptionHelpers.ThrowIfArgumentNull(topic, nameof(topic));

        return topicIndex.GetChildTopics(topic.Path);
    }

    /// <summary>
    /// Gets the collection of crumbs leading to the specified topic.
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <returns>
    ///   <para>The ordered collection of zero-or-more ancestor topics.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicIndex"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topic"/> is null.</para>
    /// </exception>
    public static IEnumerable<TopicModel> GetCrumbs(this ITopicIndex topicIndex, TopicModel topic)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicIndex, nameof(topicIndex));
        ExceptionHelpers.ThrowIfArgumentNull(topic, nameof(topic));

        var crumbs = new List<TopicModel>();
        var parentTopic = topicIndex.GetParentTopic(topic.Path);
        while (parentTopic is not null) {
            crumbs.Insert(0, parentTopic);
            parentTopic = topicIndex.GetParentTopic(parentTopic.Path);
        }
        return crumbs;
    }
}
