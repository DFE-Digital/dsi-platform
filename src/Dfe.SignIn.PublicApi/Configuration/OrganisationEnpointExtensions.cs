using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.Organisations;

namespace Dfe.SignIn.PublicApi.Configuration;

[ExcludeFromCodeCoverage]
public static class OrganisationEndpointExtensions
{
    /// <summary>
    /// Registers use cases for  organisation related public API interactions.
    /// sjw 2
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    public static void SetupOrganisationInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<GetUsersAtOrganisationUseCase>();
    }
}
