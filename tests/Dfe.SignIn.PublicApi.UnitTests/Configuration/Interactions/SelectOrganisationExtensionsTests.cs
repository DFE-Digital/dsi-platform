using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.PublicApi.Configuration.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration.Interactions;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupSelectOrganisationInteractions(IServiceCollection)

    [TestMethod]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            SelectOrganisationExtensions.SetupSelectOrganisationInteractions(
                services: null!
            )
        );
    }

    [TestMethod]
    public void SetupSelectOrganisationInteractions_SetupSelectOrganisationSessionCache()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationInteractions();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(ISelectOrganisationSessionRepository)
            )
        );
    }

    [DataRow(
        typeof(IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>),
        DisplayName = nameof(CreateSelectOrganisationSessionRequest)
    )]
    [DataRow(
        typeof(IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse>),
        DisplayName = nameof(FilterOrganisationsForUserRequest)
    )]
    [DataTestMethod]
    public void SetupSelectOrganisationInteractions_HasExpectedInteractionType(
        Type interactionType)
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationInteractions();

        Assert.IsTrue(
            services.Any(descriptor => descriptor.ServiceType == interactionType)
        );
    }

    #endregion
}
