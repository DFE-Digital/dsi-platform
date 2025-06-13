using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// Represents a service that reads topic files from the file system.
/// </summary>
public interface ITopicFileReader
{
    /// <summary>
    /// Discover the paths of all topic files at the given content path.
    /// </summary>
    /// <param name="contentFilesPath">Path to the directory that contains content files.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>A task that resolves to the array of topic file paths.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="contentFilesPath"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="contentFilesPath"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<string[]> DiscoverAllAsync(string contentFilesPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a topic file from the given file path.
    /// </summary>
    /// <param name="topicFilePath">Path to the topic file.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>A task that resolves to the read markdown encoded topic content.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicFilePath"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="topicFilePath"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///   <para>If the topic file was not found.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<string> ReadAsync(string topicFilePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// A concrete implementation of the <see cref="ITopicFileLoader"/> interface which reads
/// markdown encoded topic files.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TopicFileReader : ITopicFileReader
{
    /// <inheritdoc/>
    public Task<string[]> DiscoverAllAsync(string contentFilesPath, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(contentFilesPath, nameof(contentFilesPath));

        string[] filePaths = Directory.GetFiles(contentFilesPath, "*.md", SearchOption.AllDirectories);
        return Task.FromResult(filePaths);
    }

    /// <inheritdoc/>
    public Task<string> ReadAsync(string topicFilePath, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(topicFilePath, nameof(topicFilePath));

        return File.ReadAllTextAsync(topicFilePath, cancellationToken);
    }
}
