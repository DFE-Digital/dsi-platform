using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Base.Framework.Caching;

/// <summary>
/// Defines options for the <see cref="InMemoryInteractionCache{TRequest}"/> service.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
public sealed class InMemoryInteractionCacheOptions<TRequest>
    : IOptions<InMemoryInteractionCacheOptions<TRequest>>
    where TRequest : class, IKeyedRequest
{
    /// <summary>
    /// Gets or sets the default absolute expiration date for a cache entry.
    /// </summary>
    public DateTimeOffset? DefaultAbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the default absolute expiration time, relative to now, for a cache
    /// entry.
    /// </summary>
    public TimeSpan? DefaultAbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets the default duration at which a cache entry can remain inactive
    /// before it will be removed. This will not extend the entry lifetime beyond the
    /// absolute expiration (if set).
    /// </summary>
    public TimeSpan? DefaultSlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets a delegate which can be used to override the memory cache options
    /// for a specific request.
    /// </summary>
    /// <remarks>
    ///   <para>Default values are assumed when override delegate is not specified.</para>
    /// </remarks>
    public Func<TRequest, MemoryCacheEntryOptions>? OverrideCacheEntryOptionsForRequest { get; set; }

    /// <summary>
    /// Gets memory cache options for a request.
    /// </summary>
    /// <remarks>
    ///   <para>Default values are assumed when <see cref="OverrideCacheEntryOptionsForRequest"/>
    ///   is not specified.</para>
    /// </remarks>
    /// <param name="request">The request model.</param>
    /// <returns>
    ///   <para>The memory cache options.</para>
    ///   <para>- or </para>
    ///   <para>A value of null indicates that the response should not be cached.</para>
    /// </returns>
    public MemoryCacheEntryOptions? GetCacheEntryOptionsForRequest(TRequest request)
    {
        if (this.OverrideCacheEntryOptionsForRequest is not null) {
            return this.OverrideCacheEntryOptionsForRequest(request);
        }
        return new() {
            AbsoluteExpiration = this.DefaultAbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = this.DefaultAbsoluteExpirationRelativeToNow,
            SlidingExpiration = this.DefaultSlidingExpiration,
        };
    }

    /// <inheritdoc/>
    InMemoryInteractionCacheOptions<TRequest> IOptions<InMemoryInteractionCacheOptions<TRequest>>.Value => this;
}

/// <summary>
/// An <see cref="IInteractionCache{TRequest}"/> which caches interaction responses
/// in-memory using <see cref="MemoryCache"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <param name="optionsAccessor">Provides access to in-memory interaction cache options.</param>
/// <param name="cache">The memory cache instance.</param>
public sealed class InMemoryInteractionCache<TRequest>(
    IOptions<InMemoryInteractionCacheOptions<TRequest>> optionsAccessor,
    MemoryCache cache
) : IInteractionCache<TRequest>
    where TRequest : class, IKeyedRequest
{
    private static void CheckCacheKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) {
            throw new InvalidOperationException("Invalid cache key.");
        }
    }

    /// <inheritdoc/>
    public Task SetAsync(TRequest request, object response)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));
        ExceptionHelpers.ThrowIfArgumentNull(response, nameof(response));
        CheckCacheKey(request.Key);

        var cacheEntryOptions = optionsAccessor.Value.GetCacheEntryOptionsForRequest(request);
        if (cacheEntryOptions is not null) {
            cache.Set(request.Key, response, cacheEntryOptions);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<object?> GetAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));
        CheckCacheKey(request.Key);

        var cachedResponse = cache.Get(request.Key);
        return Task.FromResult(cachedResponse);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(key, nameof(key));

        cache.Remove(key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all interaction responses from the cache.
    /// </summary>
    public Task ClearAsync()
    {
        cache.Clear();
        return Task.CompletedTask;
    }
}
