using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.SelectOrganisation.SessionData.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.SelectOrganisation.SessionData;

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
    /// <param name="setupAction">An action to configure the provided options.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="setupAction"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddSelectOrganisationSessionCache(this IServiceCollection services, Action<SelectOrganisationSessionCacheOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();

        services.Configure(setupAction);
        services.AddSingleton<ISessionDataSerializer, DefaultSessionDataSerializer>();
        services.AddSingleton<ISelectOrganisationSessionRepository, DistributedCacheSelectOrganisationSessionRepository>();

        return services;
    }
}
