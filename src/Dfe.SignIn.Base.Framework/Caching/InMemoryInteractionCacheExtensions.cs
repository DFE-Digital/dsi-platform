using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Base.Framework.Caching;

/// <summary>
/// Extensions for adding in-memory caching for interaction requests.
/// </summary>
public static class InMemoryInteractionCacheExtensions
{
    /// <summary>
    /// Adds in-memory caching for interaction requests of a specific type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure in-memory
    /// caching options for the specific request type.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInMemoryInteractionCache<TRequest>(
        this IServiceCollection services,
        Action<InMemoryInteractionCacheOptions<TRequest>>? configureOptions = null
    ) where TRequest : class, IKeyedRequest
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        if (configureOptions is not null) {
            services.Configure(configureOptions);
        }

        services.AddSingleton<IInteractionCache<TRequest>, InMemoryInteractionCache<TRequest>>(provider => {
            var optionsAccessor = provider.GetRequiredService<IOptions<InMemoryInteractionCacheOptions<TRequest>>>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new InMemoryInteractionCache<TRequest>(optionsAccessor, cache);
        });

        services.Decorate<IInteractor<TRequest>, CachedInteractor<TRequest>>();

        return services;
    }
}
