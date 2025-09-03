using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Web.Help.Content.Processing;

namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// Represents a service that reads and processes topic files.
/// </summary>
public interface ITopicFilePipeline
{
    /// <summary>
    /// Discover and load all topic files.
    /// </summary>
    /// <param name="contentFilesPath">Path to the directory that contains content files.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>A task that resolves to the array of loaded topics.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="contentFilesPath"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="contentFilesPath"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///   <para>If one or more topic files were not found.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<TopicModel[]> LoadAllTopicFilesAsync(string contentFilesPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load an individual topic file.
    /// </summary>
    /// <param name="contentFilesPath">Path to the directory that contains content files.
    /// This is used to determine relative content paths.</param>
    /// <param name="topicFilePath">Path to the topic file.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>A task that resolve to the loaded topic model.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="contentFilesPath"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topicFilePath"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="contentFilesPath"/> is an empty string.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topicFilePath"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///   <para>If the specified file was not found.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<TopicModel> LoadTopicFileAsync(string contentFilesPath, string topicFilePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// A concrete implementation of the <see cref="ITopicFilePipeline"/> interface.
/// </summary>
public sealed class TopicFilePipeline(
    ITopicFileReader topicFileReader,
    IEnumerable<ITopicPreProcessor> topicPreProcessors,
    ITopicMarkdownProcessor topicMarkdownProcessor,
    IEnumerable<ITopicProcessor> topicProcessors
) : ITopicFilePipeline
{
    /// <inheritdoc/>
    public async Task<TopicModel[]> LoadAllTopicFilesAsync(string contentFilesPath, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(contentFilesPath, nameof(contentFilesPath));

        string[] topicFilePaths = await topicFileReader.DiscoverAllAsync(contentFilesPath, cancellationToken);
        return await Task.WhenAll(
            topicFilePaths.Select(topicFilePath => this.LoadTopicFileAsync(contentFilesPath, topicFilePath, cancellationToken))
        );
    }

    /// <inheritdoc/>
    public async Task<TopicModel> LoadTopicFileAsync(string contentFilesPath, string topicFilePath, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(contentFilesPath, nameof(contentFilesPath));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(topicFilePath, nameof(topicFilePath));

        string topicPath = TopicPathHelpers.ResolveTopicPath(Path.GetRelativePath(contentFilesPath, topicFilePath));

        string topicFileContent = await topicFileReader.ReadAsync(topicFilePath, cancellationToken);
        foreach (var preProcessor in topicPreProcessors) {
            topicFileContent = await preProcessor.ProcessAsync(topicPath, topicFileContent, cancellationToken);
        }

        var topic = await topicMarkdownProcessor.ProcessMarkdownAsync(topicPath, topicFileContent, cancellationToken);
        foreach (var processor in topicProcessors) {
            topic = await processor.ProcessAsync(topic, cancellationToken);
        }
        return topic;
    }
}
