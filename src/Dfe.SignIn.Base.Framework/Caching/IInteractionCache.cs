namespace Dfe.SignIn.Base.Framework.Caching;

/// <summary>
/// Represents a thread-safe service that can cache the responses of interaction requests.
/// </summary>
/// <remarks>
///   <h2>Note to implementors:</h2>
///   <para>Generic implementations can be created if response types are needed for
///   serialization purposes; for example:</para>
///   <code language="csharp"><![CDATA[
///       public sealed class ExampleInteractionCache<TResponse> : IInteractionCache
///           where TResponse : class
///       {
///           ...
///       }
///   ]]></code>
/// </remarks>
/// <typeparam name="TRequest">
/// The type of request being handled, which must implement <see cref="ICacheableRequest"/>.
/// </typeparam>
/// <seealso cref="ICacheableRequest"/>
public interface IInteractionCache<TRequest>
    where TRequest : ICacheableRequest
{
    /// <summary>
    /// Sets the cache entry for an interaction response.
    /// </summary>
    /// <param name="request">The interaction request model.</param>
    /// <param name="response">The response.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is a value of null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="response"/> is a value of null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the <paramref name="request"/> has a null, empty, or otherwise invalid
    ///   <see cref="ICacheableRequest.CacheKey"/> value.</para>
    /// </exception>
    Task SetAsync(TRequest request, object response);

    /// <summary>
    /// Attempts to get a response that was previously cache against the request.
    /// </summary>
    /// <param name="request">The interaction request model.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The cached response.</para>
    ///   <para>- or -</para>
    ///   <para>A value of null indicating that no response is cached.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the <paramref name="request"/> has a null, empty, or otherwise invalid
    ///   <see cref="ICacheableRequest.CacheKey"/> value.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<object?> GetAsync(TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an interaction response from the cache.
    /// </summary>
    /// <param name="key">The unique key representing the cache entry.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="key"/> is a value of null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="key"/> is an empty string.</para>
    /// </exception>
    Task RemoveAsync(string key);
}
