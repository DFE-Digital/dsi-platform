using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// A cache of processed topics that are stored in-memory providing high performance
/// access to topic content, association, and metadata.
/// </summary>
public sealed class TopicMemoryCache : ITopicIndex
{
    private readonly Dictionary<string, TopicModel> TopicsByPath;
    private readonly Dictionary<string, TopicModel[]> TopicsBySection;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicMemoryCache"/> class.
    /// </summary>
    /// <param name="topics">The enumerable collection of topics.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topics"/> is null.</para>
    /// </exception>
    public TopicMemoryCache(IEnumerable<TopicModel> topics)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topics, nameof(topics));

        this.TopicsByPath = topics.ToDictionary(topic => topic.Path, topic => topic);

        this.TopicsBySection = topics
            .Select(topic => new {
                Topic = topic,
                ParentPath = TopicPathHelpers.GetParentTopicPath(topic.Path)!,
            })
            .Where(entry => !string.IsNullOrEmpty(entry.ParentPath))
            .GroupBy(entry => entry.ParentPath, entry => entry.Topic)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }

    /// <inheritdoc/>
    public IEnumerable<TopicModel> GetAllTopics()
    {
        return this.TopicsByPath.Values.AsEnumerable();
    }

    /// <inheritdoc/>
    public TopicModel? GetTopic(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        this.TopicsByPath.TryGetValue(topicPath, out var topic);
        return topic;
    }

    /// <inheritdoc/>
    public TopicModel? GetParentTopic(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        string? parentTopicPath = TopicPathHelpers.GetParentTopicPath(topicPath);
        return parentTopicPath is not null
            ? this.GetTopic(parentTopicPath)
            : null;
    }

    /// <inheritdoc/>
    public IEnumerable<TopicModel> GetChildTopics(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        this.TopicsBySection.TryGetValue(topicPath, out var childTopics);
        return childTopics! ?? [];
    }
}
