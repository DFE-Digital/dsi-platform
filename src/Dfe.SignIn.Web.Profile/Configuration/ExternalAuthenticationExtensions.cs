using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

namespace Dfe.SignIn.Web.Profile.Configuration;

public static class ExternalAuthenticationExtensions
{
    /// <summary>
    /// The name of the redis configuration section for the MSAL token cache.
    /// </summary>
    public const string TokenRedisCacheConfigurationSectionName = "TokenRedisCache";

    public static IServiceCollection AddExternalAuthentication(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services
            .AddAuthentication()
            .AddMicrosoftIdentityWebApp(
                openIdConnectScheme: ExternalAuthConstants.OpenIdConnectSchemeName,
                cookieScheme: ExternalAuthConstants.CookiesSchemeName,
                configureMicrosoftIdentityOptions: options => {
                    configuration.GetRequiredSection("ExternalId").Bind(options);
                    options.CallbackPath = "/auth/callback";
                },
                configureCookieAuthenticationOptions: options => {
                    options.Cookie.Name = ExternalAuthConstants.CookieName;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(
                        configuration.GetValue<int>($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes")
                    );
                })
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDistributedTokenCaches();

        services.AddScoped<ISelectAssociatedAccountHelper, SelectAssociatedAccountHelper>();

        services.SetupRedisCacheStore(DistributedCacheKeys.EntraTokenCache,
            configuration.GetRequiredSection(TokenRedisCacheConfigurationSectionName));

        services.AddSingleton<IMsalTokenCacheProvider>(provider => {
            var tokenCache = provider.GetRequiredKeyedService<IDistributedCache>(DistributedCacheKeys.EntraTokenCache);
            return ActivatorUtilities.CreateInstance<MsalDistributedTokenCacheAdapter>(provider, tokenCache);
        });

        return services;
    }
}
