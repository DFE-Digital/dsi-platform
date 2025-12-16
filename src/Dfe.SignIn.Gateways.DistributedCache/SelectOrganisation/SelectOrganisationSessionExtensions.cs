using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;

/// <summary>
/// Extension methods for setting up the distributed cache implementation for
/// "select organisation" sessions.
/// </summary>
public static class SelectOrganisationSessionExtensions
{
    /// <summary>
    /// Adds distributed cache implementation of "select organisation" sessions to the
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddSelectOrganisationSessionCache(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.TryAddSingleton<ICacheEntrySerializer, DefaultCacheEntrySerializer>();
        services.AddSingleton<ISelectOrganisationSessionRepository, SelectOrganisationSessionDistributedCache>();

        return services;
    }
}
