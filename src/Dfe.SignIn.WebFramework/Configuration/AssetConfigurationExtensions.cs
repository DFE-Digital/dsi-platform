using Dfe.SignIn.Core.Framework;
using GovUk.Frontend.AspNetCore;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Extension methods for setting up frontend assets.
/// </summary>
public static class AssetConfigurationExtensions
{
    /// <summary>
    /// Setup frontend assets and GDS design system.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupFrontendAssets(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddGovUkFrontend((options) => {
            // Disable hosting of GDS design system assets since these are hosted from our CDN.
            options.StaticAssetsContentPath = null;
            options.CompiledContentPath = null;
        });
    }
}
