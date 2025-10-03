using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.WebFramework.Configuration;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up interaction request/response caching.
/// </summary>
public static class InteractionCachingExtensions
{
    /// <summary>
    /// Adds caching for requests/responses when using the interaction framework.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractionCaching(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        var interactionsRedisCacheSection = configuration.GetSection("InteractionsRedisCache");
        if (!interactionsRedisCacheSection.Exists()) {
            // Redis cache has not been configured; do nothing.
            return services;
        }

        services.SetupRedisCacheStore(
            DistributedCacheKeys.InteractionRequests,
            interactionsRedisCacheSection
        );

        services.AddDistributedInteractionCache<GetApplicationApiConfigurationRequest, GetApplicationApiConfigurationResponse>(options => {
            options.DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
        });

        return services;
    }
}
