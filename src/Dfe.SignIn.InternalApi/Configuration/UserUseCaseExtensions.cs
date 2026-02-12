using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Users;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for setting up "User" use cases.
/// </summary>
public static class UserUseCaseExtensions
{
    /// <summary>
    /// Adds use case interactors.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddUserUseCases(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.AddInteractor<AutoLinkEntraUserToDsiUseCase>();
        services.AddInteractor<ChangeJobTitleUseCase>();
        services.AddInteractor<CreateUserUseCase>();
        services.AddInteractor<GetOrganisationsAssociatedWithUserUseCase>();
        services.AddInteractor<GetUserProfileUseCase>();
        services.AddInteractor<GetUserStatusUseCase>();

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
