using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension methods for setting up the authentication organisation selector.
/// </summary>
public static class SelectOrganisationExtensions
{
    /// <summary>
    /// Setup dependencies for DfE Sign-in "select organisation" features.
    /// </summary>
    /// <remarks>
    ///   <para>It is also necessary to setup the DfE Sign-in Public API with
    ///   <see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/>.</para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupSelectOrganisationFeatures(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        // General "select organisation" features.
        services.AddScoped<ISelectOrganisationUserFlow, StandardSelectOrganisationUserFlow>();

        // Features that enable "select organisation" to be integrated into the
        // user authentication journey.
        services.AddScoped<ISelectOrganisationEvents, StandardSelectOrganisationEvents>();
        services.AddScoped<StandardSelectOrganisationMiddleware>();

        return services;
    }
}
