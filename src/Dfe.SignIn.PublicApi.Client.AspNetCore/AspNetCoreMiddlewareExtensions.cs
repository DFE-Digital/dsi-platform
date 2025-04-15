using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Extension methods for setting up the authentication organisation selector with ASP.NET Core.
/// </summary>
public static class AspNetCoreMiddlewareExtensions
{
    /// <summary>
    /// Use the DfE Sign-in organisation selector when authenticated a user.
    /// </summary>
    /// <remarks>
    ///   <para>Refer to <see cref="AuthenticationOrganisationSelectorMiddleware"/>
    ///   for more details on how the middleware works.</para>
    ///   <para>Dependencies must be set up with the following:</para>
    ///   <list type="bullet">
    ///     <item><see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/></item>
    ///     <item><see cref="AuthenticationOrganisationSelectorExtensions.SetupSelectOrganisationFeatures(IServiceCollection)"/></item>
    ///   </list>
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="app"/> is null.</para>
    /// </exception>
    public static void UseAuthenticationOrganisationSelectorMiddleware(this IApplicationBuilder app)
    {
        ExceptionHelpers.ThrowIfArgumentNull(app, nameof(app));

        Func<IHttpMiddleware> middlewareFactory = app.ApplicationServices.GetRequiredService<
            AuthenticationOrganisationSelectorMiddleware
        >;
        app.UseMiddleware<HttpMiddlewareAspNetCoreAdapter>(middlewareFactory);
    }
}
