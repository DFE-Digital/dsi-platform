using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.DistributedCache.Interactions;

/// <summary>
/// Defines options for the <see cref="InteractionDistributedCache{TRequest, TResponse}"/> service.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
public sealed class InteractionDistributedCacheOptions<TRequest>
    : IOptions<InteractionDistributedCacheOptions<TRequest>>
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
    /// Gets or sets a delegate which can be used to override the distributed cache
    /// options for a specific request.
    /// </summary>
    /// <remarks>
    ///   <para>Default values are assumed when override delegate is not specified.</para>
    /// </remarks>
    public Func<TRequest, DistributedCacheEntryOptions>? OverrideCacheEntryOptionsForRequest { get; set; }

    /// <summary>
    /// Gets distributed cache options for a request.
    /// </summary>
    /// <remarks>
    ///   <para>Default values are assumed when <see cref="OverrideCacheEntryOptionsForRequest"/>
    ///   is not specified.</para>
    /// </remarks>
    /// <param name="request">The request model.</param>
    /// <returns>
    ///   <para>The distributed cache options.</para>
    ///   <para>- or </para>
    ///   <para>A value of null indicates that the response should not be cached.</para>
    /// </returns>
    public DistributedCacheEntryOptions? GetCacheEntryOptionsForRequest(TRequest request)
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
    InteractionDistributedCacheOptions<TRequest> IOptions<InteractionDistributedCacheOptions<TRequest>>.Value => this;
}

/// <summary>
/// An <see cref="IInteractionCache{TRequest}"/> which caches interaction responses
/// in-memory using <see cref="IDistributedCache"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <param name="optionsAccessor">Provides access to in-memory interaction cache options.</param>
/// <param name="serializer">Service that serialzies cache entries.</param>
/// <param name="cache">The memory cache instance.</param>
public sealed partial class InteractionDistributedCache<TRequest, TResponse>(
    IOptions<InteractionDistributedCacheOptions<TRequest>> optionsAccessor,
    [FromKeyedServices(DistributedCacheKeys.InteractionRequests)] IDistributedCache cache,
    ICacheEntrySerializer serializer
) : IInteractionCache<TRequest>
    where TRequest : class, IKeyedRequest
    where TResponse : class
{
    private static void CheckCacheKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) {
            throw new InvalidOperationException("Invalid cache key.");
        }
    }

    // Cache key transformation "{RequestName}:{Request.CacheKey}"
    [GeneratedRegex("Request$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
    private static readonly string RequestKeyPrefix = MyRegex().Replace(typeof(TRequest).Name, "");
    private static string TransformCacheKey(string key) => $"{RequestKeyPrefix}:{key}";

    /// <inheritdoc/>
    public Task SetAsync(TRequest request, object response)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));
        ExceptionHelpers.ThrowIfArgumentNull(response, nameof(response));
        CheckCacheKey(request.Key);

        var cacheEntryOptions = optionsAccessor.Value.GetCacheEntryOptionsForRequest(request);
        if (cacheEntryOptions is not null) {
            string cacheKey = TransformCacheKey(request.Key);
            string entryJson = serializer.Serialize(response);
            cache.SetStringAsync(cacheKey, entryJson, cacheEntryOptions);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<object?> GetAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));
        CheckCacheKey(request.Key);

        string cacheKey = TransformCacheKey(request.Key);

        string? responseJson = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (responseJson is null) {
            return null;
        }

        return serializer.Deserialize<TResponse>(responseJson);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(key, nameof(key));

        string cacheKey = TransformCacheKey(key);

        return cache.RemoveAsync(cacheKey);
    }
}
