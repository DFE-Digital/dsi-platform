namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// Represents a service that provides access to a <see cref="ITopicIndex"/>.
/// </summary>
public interface ITopicIndexAccessor
{
    /// <summary>
    /// Gets the topic index.
    /// </summary>
    /// <remarks>
    ///   <para>Calling this method with <c>true</c> for <paramref name="invalidate"/>
    ///   is a slow operation since any cache will need to be rebuilt. This feature is
    ///   useful for local development since it provides a convenient way for developers
    ///   to review content changes without having to restart the application.</para>
    /// </remarks>
    /// <param name="invalidate">Indicates if cache should be invalidated.</param>
    /// <returns>
    ///   <para>The topic index.</para>
    /// </returns>
    Task<ITopicIndex> GetIndexAsync(bool invalidate = false);
}
