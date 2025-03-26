using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

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
    ///   <para>Enable the "select organisation" middleware with
    ///   <see cref="UseAuthenticationOrganisationSelectorMiddleware(IApplicationBuilder)"/>.</para>
    /// </remarks>
    /// <param name="services"></param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSelectOrganisationFeatures(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // General "select organisation" features.
        services.AddSingleton<ISelectOrganisationCallbackProcessor, SelectOrganisationCallbackProcessor>();

        // Features that enable "select organisation" to be integrated into the
        // user authentication journey.
        services.AddSingleton<IAuthenticationOrganisationSelector, AuthenticationOrganisationSelector>();
        services.AddSingleton<IOrganisationClaimManager, OrganisationClaimManager>();
    }

    /// <summary>
    /// Use the DfE Sign-in organisation selector when authenticated a user.
    /// </summary>
    /// <remarks>
    ///   <para>Refer to <see cref="AuthenticationOrganisationSelectorMiddleware"/>
    ///   for more details on how the middleware works.</para>
    ///   <para>Dependencies must be set up with the following:</para>
    ///   <list type="bullet">
    ///     <item><see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/></item>
    ///     <item><see cref="SetupSelectOrganisationFeatures(IServiceCollection)"/></item>
    ///   </list>
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="app"/> is null.</para>
    /// </exception>
    public static void UseAuthenticationOrganisationSelectorMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        app.UseMiddleware<AuthenticationOrganisationSelectorMiddleware>();
    }
}
