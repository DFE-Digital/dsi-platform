using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;
using Owin;

namespace Dfe.SignIn.PublicApi.Client.Owin;

/// <summary>
/// Extension methods for setting up the authentication organisation selector with OWIN.
/// </summary>
public static class OwinMiddlewareExtensions
{
    /// <summary>
    /// Use the DfE Sign-in organisation selector when authenticated a user.
    /// </summary>
    /// <remarks>
    ///   <para>Refer to <see cref="StandardSelectOrganisationMiddleware"/>
    ///   for more details on how the middleware works.</para>
    ///   <para>Dependencies must be set up with the following:</para>
    ///   <list type="bullet">
    ///     <item><see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/></item>
    ///     <item><see cref="SelectOrganisationExtensions.SetupSelectOrganisationFeatures(IServiceCollection)"/></item>
    ///   </list>
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <param name="middlewareFactory">Represents a factory method that instantiates middleware.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="app"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="middlewareFactory"/> is null.</para>
    /// </exception>
    public static void UseAuthenticationOrganisationSelectorMiddleware(
        this IAppBuilder app,
        Func<StandardSelectOrganisationMiddleware> middlewareFactory)
    {
        ExceptionHelpers.ThrowIfArgumentNull(app, nameof(app));
        ExceptionHelpers.ThrowIfArgumentNull(middlewareFactory, nameof(middlewareFactory));

        app.Use(async (context, next) => {
            var adapter = new HttpMiddlewareOwinAdapter(middlewareFactory());
            await adapter.InvokeAsync(context, next);
        });
    }
}
