using GovUk.Frontend.AspNetCore;

namespace Dfe.SignIn.SelectOrganisation.Web.Configuration;

/// <summary>
/// Extension methods for setting up frontend assets.
/// </summary>
public static class AssetConfigurationExtensions
{
    /// <summary>
    /// Setup frontend assets and GDS design system.
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
    public static IServiceCollection AddFrontendAssets(this IServiceCollection services, Action<AssetOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();

        services.Configure(setupAction);
        services.AddGovUkFrontend((options) => {
            // Disable hosting of GDS design system assets since these are hosted from our CDN.
            options.StaticAssetsContentPath = null;
            options.CompiledContentPath = null;
        });

        return services;
    }
}
