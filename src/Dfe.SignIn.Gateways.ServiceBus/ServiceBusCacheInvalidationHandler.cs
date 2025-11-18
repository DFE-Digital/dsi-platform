using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Represents a delegate that gets the cache key for a given service bus message.
/// </summary>
/// <param name="message">The Service Bus message.</param>
/// <returns>
///   <para>The cache key where possible; otherwise, a value of null indciating that the
///   cache key could not be resolved.</para>
/// </returns>
public delegate string? CacheKeyFromServiceBusMessageDelegate(ServiceBusReceivedMessage message);

/// <summary>
/// A Service Bus message handler which invalidates a cached interaction response.
/// </summary>
/// <typeparam name="TRequest">The type of interaction request.</typeparam>
/// <param name="interactionCache">The interaction cache.</param>
/// <param name="getCacheKey">A delegate to get a cache key from the Service Bus message.</param>
public sealed class ServiceBusCacheInvalidationHandler<TRequest>(
    IInteractionCache<TRequest> interactionCache,
    CacheKeyFromServiceBusMessageDelegate getCacheKey
) : IServiceBusMessageHandler
    where TRequest : class, IKeyedRequest
{
    /// <inheritdoc/>
    public Task HandleAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
    {
        string? cacheKey = getCacheKey(message);
        if (cacheKey is not null) {
            return interactionCache.RemoveAsync(cacheKey);
        }
        return Task.CompletedTask;
    }
}
