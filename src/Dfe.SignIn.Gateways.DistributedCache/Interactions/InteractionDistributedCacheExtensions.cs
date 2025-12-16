using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.SignIn.Gateways.DistributedCache.Interactions;

/// <summary>
/// Extensions for adding in-memory caching for interaction requests.
/// </summary>
public static class InteractionDistributedCacheExtensions
{
    /// <summary>
    /// Adds in-memory caching for interaction requests of a specific type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure in-memory
    /// caching options for the specific request type.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddDistributedInteractionCache<TRequest, TResponse>(
        this IServiceCollection services,
        Action<InteractionDistributedCacheOptions<TRequest>>? configureOptions = null
    )
        where TRequest : class, IKeyedRequest
        where TResponse : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        if (configureOptions is not null) {
            services.Configure(configureOptions);
        }

        services.TryAddSingleton<ICacheEntrySerializer, DefaultCacheEntrySerializer>();
        services.AddSingleton<IInteractionCache<TRequest>, InteractionDistributedCache<TRequest, TResponse>>();

        services.Decorate<IInteractor<TRequest>, CachedInteractor<TRequest>>();

        return services;
    }
}
