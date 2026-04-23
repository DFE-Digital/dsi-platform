using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Users;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up "user" features.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Registers all endpoints for the "user" feature.
    /// </summary>
    /// <param name="services"></param>
    [ExcludeFromCodeCoverage]
    public static IServiceCollection SetupUserInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<GetServiceUsersUseCase>();

        return services;
    }
}
