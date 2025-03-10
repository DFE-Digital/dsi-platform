using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Dfe.SignIn.SelectOrganisation.SessionData;

namespace Dfe.SignIn.PublicApi.Configuration.Interactions;

/// <exclude/>
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
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        SetupRedisSessionStore(services);

        services.AddInteractor<CreateSelectOrganisationSession_UseCase>();
        services.AddInteractor<FilterOrganisationsForUser_UseCase>();
    }

    private static void SetupRedisSessionStore(this IServiceCollection services)
    {
        services.AddStackExchangeRedisCache(options => {
            options.Configuration = "localhost:6379";
        });
        services.AddSelectOrganisationSessionCache(options => { });
    }
}
