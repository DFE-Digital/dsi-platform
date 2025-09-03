using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Extension methods for setting up redis session stores.
/// </summary>
[ExcludeFromCodeCoverage]
public static class RedisExtensions
{
    /// <summary>
    /// Setup redis session store with the given cache key.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="cacheStoreKey">The cache store key.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>The service collection instance for chaining.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="cacheStoreKey"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupRedisSessionStore(
        this IServiceCollection services, string cacheStoreKey, IConfiguration configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(cacheStoreKey, nameof(cacheStoreKey));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.AddKeyedSingleton<IDistributedCache, RedisCache>(
            serviceKey: cacheStoreKey,
            implementationFactory: (provider, key) => {
                string connectionString = configuration.GetValue<string>("ConnectionString")
                    ?? throw new InvalidOperationException("Missing connection string for Redis.");

                var configOptions = ConfigurationOptions.Parse(connectionString);
                configOptions.DefaultDatabase = configuration.GetValue<int>("DatabaseNumber");

                var redisOptions = new RedisCacheOptions {
                    InstanceName = configuration.GetValue<string>("InstanceName"),
                    ConfigurationOptions = configOptions
                };

                return new RedisCache(Options.Create(redisOptions));
            }
        );

        return services;
    }
}
