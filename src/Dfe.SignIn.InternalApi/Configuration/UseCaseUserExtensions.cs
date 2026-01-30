using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Users;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for setting up "User" use cases.
/// </summary>
public static class UseCaseUserExtensions
{
    /// <summary>
    /// Adds use case interactors.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    public static IServiceCollection AddUseCasesUser(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddInteractor<AutoLinkEntraUserToDsiUseCase>();
        services.AddInteractor<GetUserProfileUseCase>();

        services
            .Configure<BlockedEmailAddressOptions>(options => {
                var section = configuration.GetSection("BlockedEmailAddresses");
                options.BlockedDomains = section.GetJsonList("BlockedDomains");
                options.BlockedNames = section.GetJsonList("BlockedNames");
            })
            .AddInteractor<CheckIsBlockedEmailAddressUseCase>();

        return services;
    }
}
