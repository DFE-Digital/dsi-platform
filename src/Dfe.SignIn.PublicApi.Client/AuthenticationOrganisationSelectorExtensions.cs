using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension methods for setting up the authentication organisation selector.
/// </summary>
public static class AuthenticationOrganisationSelectorExtensions
{
    /// <summary>
    /// Setup dependencies for DfE Sign-in "select organisation" features.
    /// </summary>
    /// <remarks>
    ///   <para>It is also necessary to setup the DfE Sign-in Public API with
    ///   <see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/>.</para>
    /// </remarks>
    /// <param name="services"></param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSelectOrganisationFeatures(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        // General "select organisation" features.
        services.AddSingleton<ISelectOrganisationCallbackProcessor, SelectOrganisationCallbackProcessor>();

        // Features that enable "select organisation" to be integrated into the
        // user authentication journey.
        services.AddSingleton<IAuthenticationOrganisationSelector, AuthenticationOrganisationSelector>();
        services.AddSingleton<IActiveOrganisationProvider, ActiveOrganisationClaimsProvider>();

        services.AddTransient<AuthenticationOrganisationSelectorMiddleware>();
    }
}
