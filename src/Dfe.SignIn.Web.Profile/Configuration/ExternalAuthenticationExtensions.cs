using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

namespace Dfe.SignIn.Web.Profile.Configuration;

public static class ExternalAuthenticationExtensions
{
    public static IServiceCollection AddExternalAuthentication(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
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
                        configuration.GetValue<int>("Session:DurationInMinutes")
                    );
                })
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDistributedTokenCaches();

        services.AddScoped<ISelectAssociatedAccountHelper, SelectAssociatedAccountHelper>();

        services.SetupRedisCacheStore(DistributedCacheKeys.EntraTokenCache,
            configuration.GetRequiredSection("TokenRedisCache"));

        services.AddSingleton<IMsalTokenCacheProvider>(provider => {
            var tokenCache = provider.GetRequiredKeyedService<IDistributedCache>(DistributedCacheKeys.EntraTokenCache);
            return ActivatorUtilities.CreateInstance<MsalDistributedTokenCacheAdapter>(provider, tokenCache);
        });

        return services;
    }
}
