using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Dfe.SignIn.SelectOrganisation.Web.Configuration.Interactions;

/// <exclude/>
public static class SelectOrganisationExtensions
{
    /// <summary>
    /// Setup redis session store for "select organisation" sessions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>The service collection instance for chaining.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupRedisSessionStore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services.AddKeyedSingleton<IDistributedCache, RedisCache>(
            serviceKey: SelectOrganisationConstants.CacheStoreKey,
            implementationFactory: (provider, key) => {

                // Initialize ConfigurationOptions separately
                var configOptions = ConfigurationOptions.Parse(configuration.GetValue<string>("ConnectionString"));
                configOptions.DefaultDatabase = configuration.GetValue<int>("DatabaseNumber");

                var redisOptions = new RedisCacheOptions
                {
                    InstanceName = configuration.GetValue<string>("InstanceName"),
                    ConfigurationOptions = configOptions
                };

                return new RedisCache(Options.Create(redisOptions));
            }
        );

        services.AddSelectOrganisationSessionCache();

        return services;
    }

    /// <summary>
    /// Setup "select organisation" interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSelectOrganisationInteractions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddInteractor<GetSelectOrganisationSessionByKey_UseCase>();
        services.AddInteractor<InvalidateSelectOrganisationSession_UseCase>();
    }
}
