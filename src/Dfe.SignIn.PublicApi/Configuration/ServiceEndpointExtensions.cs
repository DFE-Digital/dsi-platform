using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.PublicApi;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up service-related interactions and endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceEndpointExtensions
{
    /// <summary>
    /// Registers use cases for service-related public API interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    public static void SetupServiceInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<GetUserAccessToServiceUseCase>();
    }
}
