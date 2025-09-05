using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up "select organisation" features.
/// </summary>
public static class SelectOrganisationExtensions
{
    /// <summary>
    /// Setup "select organisation" interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSelectOrganisationInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<CreateSelectOrganisationSessionUseCase>();
        services.AddInteractor<FilterOrganisationsForUserUseCase>();
    }
}
