using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.DistributedCache.Interactions;

/// <summary>
/// Provides extension methods for configuring and retrieving interaction limiter options.
/// </summary>
public static class DistributedCacheInteractionLimiterExtensions
{
    /// <summary>
    /// Adds an interaction limiter for the specified request type to the service collection.
    /// </summary>
    /// <typeparam name="TRequest">The type of the keyed request.</typeparam>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>The service collection instance for chaining.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractionLimiter<TRequest>(
        this IServiceCollection services, IConfigurationRoot configuration)
        where TRequest : IKeyedRequest
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.Configure<DistributedCacheInteractionLimiterOptions>(
            typeof(TRequest).Name,
            configuration.GetSection($"InteractionLimiter:{typeof(TRequest).Name}")
        );

        services.TryAddSingleton<IInteractionLimiter, DistributedCacheInteractionLimiter>();

        return services;
    }

    /// <summary>
    /// Retrieves the interaction limiter options for the specified request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the keyed request.</typeparam>
    /// <param name="optionsAccessor">The options monitor instance.</param>
    /// <returns>
    ///   <para>The limiter options for the specified request type.</para>
    /// </returns>
    public static DistributedCacheInteractionLimiterOptions Get<TRequest>(
        this IOptionsMonitor<DistributedCacheInteractionLimiterOptions> optionsAccessor)
        where TRequest : IKeyedRequest
    {
        ExceptionHelpers.ThrowIfArgumentNull(optionsAccessor, nameof(optionsAccessor));

        return optionsAccessor.Get(typeof(TRequest).Name);
    }
}
