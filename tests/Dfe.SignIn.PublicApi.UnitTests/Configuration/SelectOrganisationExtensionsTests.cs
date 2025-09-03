using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupSelectOrganisationInteractions(IServiceCollection)

    [TestMethod]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SelectOrganisationExtensions.SetupSelectOrganisationInteractions(
                services: null!
            ));
    }

    [DataRow(
        typeof(IInteractor<CreateSelectOrganisationSessionRequest>),
        DisplayName = nameof(CreateSelectOrganisationSessionRequest)
    )]
    [DataRow(
        typeof(IInteractor<FilterOrganisationsForUserRequest>),
        DisplayName = nameof(FilterOrganisationsForUserRequest)
    )]
    [DataTestMethod]
    public void SetupSelectOrganisationInteractions_HasExpectedInteractionType(
        Type interactionType)
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationInteractions();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == interactionType
            )
        );
    }

    #endregion
}
