namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// A service that loads and caches content topics.
/// </summary>
public sealed class TopicMemoryCacheAccessor(
    ITopicFilePipeline pipeline
) : ITopicIndexAccessor
{
    private ITopicIndex? topicIndex;

    /// <inheritdoc/>
    public async Task<ITopicIndex> GetIndexAsync(bool invalidate = false)
    {
        if (invalidate || this.topicIndex is null) {
            string contentFilesPath = Path.Join(Directory.GetCurrentDirectory(), "ContentFiles");
            var topics = await pipeline.LoadAllTopicFilesAsync(contentFilesPath);
            var cache = new TopicMemoryCache(topics);
            this.topicIndex = cache;
        }
        return this.topicIndex;
    }
}
