using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.Gateways.Entra.ChangeName;
using Dfe.SignIn.Gateways.Entra.ChangePassword;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.Entra;

/// <summary>
/// Service collection extension methods for registering Entra gateway services.
/// </summary>
public static class EntraGatewayExtensions
{
    /// <summary>
    /// Registers Entra application (daemon) services for background and API operations.
    /// </summary>
    public static IServiceCollection AddEntraApplicationServices(
        this IServiceCollection services,
        Action<EntraApplicationSettings> settings)
    {
        services.AddOptions<EntraApplicationSettings>()
            .Configure(settings)
            .ValidateDataAnnotations();

        services.AddSingleton<IApplicationGraphClientProvider, ApplicationGraphClientProvider>();
        services.AddScoped<IEntraChangeEmailService, EntraChangeEmailService>();
        services.AddScoped<IEntraChangeNameService, EntraChangeNameService>();

        return services;
    }

    /// <summary>
    /// Registers Entra delegated services that operate on behalf of a user.
    /// </summary>
    public static IServiceCollection AddEntraDelegatedServices(this IServiceCollection services)
    {
        services.AddScoped<IEntraChangePasswordService, EntraChangePasswordService>();
        return services;
    }
}
