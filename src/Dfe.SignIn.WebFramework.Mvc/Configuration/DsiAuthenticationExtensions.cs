using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Provides extension methods for configuring authentication in an ASP.NET Core
/// application using the DfE Sign-In identity provider.
/// </summary>
/// <remarks>
///   <para>These extensions simplify the integration of DfE Sign-In by adding and
///   configuring OpenID Connect (OIDC) authentication based on application
///   configuration.</para>
/// </remarks>
public static partial class DsiAuthenticationExtensions
{
    /// <summary>
    /// The name of the redis configuration section for integration with DfE Sign-In OIDC.
    /// </summary>
    public const string OidcConfigurationSectionName = "Oidc";

    /// <summary>
    /// Add and configure integration with the DfE Sign-In identity provider.
    /// </summary>
    /// <remarks>
    ///   <para>This method makes use of the following configuration sections:</para>
    ///   <list type="bullet">
    ///     <item><c>"Oidc"</c> - Configures the identity provider.</item>
    ///     <item><c>"Session"</c> (<see cref="UserSessionOptions"/>) - Configures user sessions.</item>
    ///   </list>
    /// </remarks>
    /// <param name="services">The collection of services to add to.</param>
    /// <param name="configuration">The root configuration section.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddDsiAuthentication(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.Configure<ApplicationOidcOptions>(
            configuration.GetRequiredSection(OidcConfigurationSectionName));

        services
            .AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
                options.CallbackPath = new PathString("/auth/cb");
                options.SignedOutCallbackPath = new PathString("/signout/complete");

                configuration.GetRequiredSection("Oidc").Bind(options);

                options.SaveTokens = true;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.CorrelationCookie.Name = "dsi-correlation-id-";
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                options.NonceCookie.Name = "dsi-nonce-";
                options.NonceCookie.HttpOnly = true;
                options.NonceCookie.IsEssential = true;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Events.OnRemoteFailure = context => {
                    string? errorCode = context.Request.Query["error"];
                    if (SessionExpiredErrorCodeRegex().IsMatch(errorCode ?? "")) {
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                    }
                    return Task.CompletedTask;
                };

                options.Events.OnSignedOutCallbackRedirect = context => {
                    var platformOptions = context.HttpContext.RequestServices
                        .GetRequiredService<IOptionsMonitor<PlatformOptions>>().CurrentValue;
                    context.Response.Redirect(platformOptions.ServicesUrl.ToString());
                    return Task.CompletedTask;
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                options.Cookie.Name = "dsi-auth";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(
                    configuration.GetValue<int>($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes")
                );

                options.Events.OnRedirectToAccessDenied = context => {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                };
            });

        return services;
    }

    [GeneratedRegex("session-?expired")]
    private static partial Regex SessionExpiredErrorCodeRegex();
}
