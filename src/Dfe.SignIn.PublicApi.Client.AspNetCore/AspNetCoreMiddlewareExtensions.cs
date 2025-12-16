using Dfe.SignIn.Base.Framework;
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
    /// Setup middleware for the "select organisation" feature which has been adapted to
    /// work with ASP.NET Core.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupSelectOrganisationMiddleware(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddScoped(provider => {
            var inner = provider.GetRequiredService<StandardSelectOrganisationMiddleware>();
            return new AdaptedSelectOrganisationMiddleware(inner);
        });

        return services;
    }

    /// <summary>
    /// Use the DfE Sign-in organisation selector when authenticated a user.
    /// </summary>
    /// <remarks>
    ///   <para>Refer to <see cref="StandardSelectOrganisationMiddleware"/> for more
    ///   details on how the middleware works.</para>
    ///   <para>The following dependencies must also be setup:</para>
    ///   <list type="bullet">
    ///     <item><see cref="PublicApiExtensions.SetupDfePublicApiClient(IServiceCollection)"/></item>
    ///     <item><see cref="SelectOrganisationExtensions.SetupSelectOrganisationFeatures(IServiceCollection)"/></item>
    ///   </list>
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="app"/> is null.</para>
    /// </exception>
    public static void UseSelectOrganisationMiddleware(this IApplicationBuilder app)
    {
        ExceptionHelpers.ThrowIfArgumentNull(app, nameof(app));

        app.UseMiddleware<AdaptedSelectOrganisationMiddleware>();
    }
}
