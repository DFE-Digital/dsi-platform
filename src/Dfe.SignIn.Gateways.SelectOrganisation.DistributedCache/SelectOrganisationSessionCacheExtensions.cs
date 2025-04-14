using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;

/// <summary>
/// Extension methods for setting up the distributed cache implementation for
/// "select organisation" sessions.
/// </summary>
public static class SelectOrganisationSessionCacheExtensions
{
    /// <summary>
    /// Adds distributed cache implementation of "select organisation" sessions to the
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddSelectOrganisationSessionCache(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddSingleton<ISessionDataSerializer, DefaultSessionDataSerializer>();
        services.AddSingleton<ISelectOrganisationSessionRepository, DistributedCacheSelectOrganisationSessionRepository>();

        return services;
    }
}
