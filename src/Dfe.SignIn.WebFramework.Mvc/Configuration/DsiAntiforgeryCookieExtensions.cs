
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Extension method for configuring the dsi-antiforgery cookie.
/// </summary>
public static class DsiAntiforgeryCookieExtensions
{
    /// <summary>
    /// Configure the dsi-antiforgery cookie.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection ConfigureDsiAntiforgeryCookie(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddAntiforgery(options => {
            options.Cookie.Name = "dsi-antiforgery";

            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services;
    }
}
