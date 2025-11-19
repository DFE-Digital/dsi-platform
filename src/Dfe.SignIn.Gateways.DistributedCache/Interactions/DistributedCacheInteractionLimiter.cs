using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.DistributedCache.Interactions;

/// <summary>
/// Options for configuring the distributed cache interaction limiter.
/// </summary>
public sealed class DistributedCacheInteractionLimiterOptions : IOptions<DistributedCacheInteractionLimiterOptions>
{
    /// <summary>
    /// Gets or sets the time period (in seconds) for limiting interactions.
    /// Default is 1 hour.
    /// </summary>
    public double TimePeriodInSeconds { get; set; } = TimeSpan.FromHours(1).TotalSeconds;

    /// <summary>
    /// Gets or sets the maximum number of interactions allowed per time period.
    /// Default is 3.
    /// </summary>
    public int InteractionsPerTimePeriod { get; set; } = 3;

    /// <inheritdoc/>
    DistributedCacheInteractionLimiterOptions IOptions<DistributedCacheInteractionLimiterOptions>.Value => this;
}

/// <summary>
/// Implements an interaction limiter using a distributed cache.
/// </summary>
/// <param name="limiterOptionsAccessor">Accessor for limiter options.</param>
/// <param name="cache">The distributed cache instance.</param>
/// <param name="timeProvider">Provides the current time.</param>
public sealed class DistributedCacheInteractionLimiter(
    IOptionsMonitor<DistributedCacheInteractionLimiterOptions> limiterOptionsAccessor,
    [FromKeyedServices(DistributedCacheKeys.GeneralCache)] IDistributedCache cache,
    TimeProvider timeProvider
) : IInteractionLimiter
{
    /// <summary>
    /// Represents an interaction limiter cache entry.
    /// </summary>
    public sealed record CacheEntry
    {
        /// <summary>
        /// The interaction request counter.
        /// </summary>
        public required int Counter { get; init; }

        /// <summary>
        /// The absolute timestamp in UTC when the request counter expires.
        /// </summary>
        public required DateTime Expires { get; init; }
    }

    /// <inheritdoc/>
    public async Task<InteractionLimiterResult> LimitActionAsync(IKeyedRequest request)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        string requestTypeName = request.GetType().Name;
        string key = $"Limiter:{requestTypeName}:{request.Key}";
        var options = limiterOptionsAccessor.Get(requestTypeName);

        var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(await cache.GetStringAsync(key) ?? "null")
            ?? new CacheEntry {
                Counter = 0,
                Expires = timeProvider.GetUtcNow().UtcDateTime
                    .AddSeconds(options.TimePeriodInSeconds),
            };

        if (cacheEntry.Counter + 1 > options.InteractionsPerTimePeriod) {
            return new InteractionLimiterResult { WasRejected = true };
        }

        await cache.SetStringAsync(key, JsonSerializer.Serialize(new CacheEntry {
            Counter = cacheEntry.Counter + 1,
            Expires = cacheEntry.Expires,
        }), new DistributedCacheEntryOptions { AbsoluteExpiration = cacheEntry.Expires });

        return new InteractionLimiterResult { WasRejected = false };
    }

    /// <inheritdoc/>
    public async Task ResetLimitAsync(IKeyedRequest request)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        string requestTypeName = request.GetType().Name;
        string key = $"Limiter:{requestTypeName}:{request.Key}";

        await cache.RemoveAsync(key, CancellationToken.None);
    }
}
