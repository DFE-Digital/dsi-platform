using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Applications;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up "application" features.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Registers all endpoints for the "select organisation" feature.
    /// </summary>
    /// <param name="services"></param>
    [ExcludeFromCodeCoverage]
    public static IServiceCollection SetupApplicationInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<GetApplicationRolesUseCase>();

        return services;
    }
}
