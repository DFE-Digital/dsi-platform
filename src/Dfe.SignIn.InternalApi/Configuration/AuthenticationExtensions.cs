using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for configuring authentication and authorization in the internal API.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "We could come back and test this, but it is not worth the effort for now.")]
public static class AuthenticationExtensions
{
    private const string LocalScheme = "LocalDev";
    private const string LocalBypassSection = "Authentication:LocalBypass";

    /// <summary>
    /// Adds authentication and authorization services to the internal API, with support for local development bypass.
    /// </summary>
    /// <param name="services">The service collection to add authentication and authorization services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInternalApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowLocalBypass = configuration.BypassLocalAuth(environment);
        var defaultScheme = allowLocalBypass
            ? LocalScheme
            : JwtBearerDefaults.AuthenticationScheme;

        var auth = services.AddAuthentication(defaultScheme);

        if (allowLocalBypass) {
            auth.AddScheme<AuthenticationSchemeOptions, LocalDevAuthHandler>(LocalScheme, _ => { });
        }
        else {
            auth.AddJwtBearer(options => {
                var section = configuration.GetRequiredSection("AzureAd");
                var instance = section.GetRequiredValue("Instance").TrimEnd('/');
                var tenantId = section.GetRequiredValue("TenantId");
                options.Audience = section.GetRequiredValue("Audience");
                options.MetadataAddress = $"{instance}/{tenantId}/.well-known/openid-configuration";
            });
        }

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    private static bool BypassLocalAuth(this IConfiguration configuration, IHostEnvironment environment)
    {
        var isDevMachine = environment.IsDevelopment() || environment.IsEnvironment("Local");
        return isDevMachine && configuration.GetValue<bool>(LocalBypassSection);
    }

    private static string GetRequiredValue(this IConfigurationSection section, string key)
        => section.GetValue<string>(key)
           ?? throw new InvalidOperationException($"Missing configuration: AzureAd:{key}");
}
