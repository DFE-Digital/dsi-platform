using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.SelectOrganisation.Web.Configuration.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.Configuration.Interactions;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupSelectOrganisationInteractions(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        SelectOrganisationExtensions.SetupSelectOrganisationInteractions(null!);
    }

    [DataRow(
        typeof(IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>),
        DisplayName = nameof(GetSelectOrganisationSessionByKeyRequest)
    )]
    [DataRow(
        typeof(IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>),
        DisplayName = nameof(InvalidateSelectOrganisationSessionRequest)
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
