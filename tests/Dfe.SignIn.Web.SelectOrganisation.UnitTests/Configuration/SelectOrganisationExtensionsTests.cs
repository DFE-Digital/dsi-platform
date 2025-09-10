using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Web.SelectOrganisation.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Web.SelectOrganisation.UnitTests.Configuration;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupSelectOrganisationInteractions(IServiceCollection)

    [TestMethod]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SelectOrganisationExtensions.SetupSelectOrganisationInteractions(null!));
    }

    [DataRow(
        typeof(IInteractor<GetSelectOrganisationSessionByKeyRequest>),
        DisplayName = nameof(GetSelectOrganisationSessionByKeyRequest)
    )]
    [DataRow(
        typeof(IInteractor<InvalidateSelectOrganisationSessionRequest>),
        DisplayName = nameof(InvalidateSelectOrganisationSessionRequest)
    )]
    [TestMethod]
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
