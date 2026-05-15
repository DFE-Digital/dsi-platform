using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Users;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up user-related interactions and endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UserEndpointExtensions
{
    /// <summary>
    /// Registers use cases for user-related public API interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    public static void SetupUserInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<GetUserOrganisationsUseCase>();
    }
}
