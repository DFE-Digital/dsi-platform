using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class AuthenticationOrganisationSelectorExtensionsTests
{
    #region SetupSelectOrganisationFeatures(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationFeatures_Throws_WhenServicesArgumentIsNull()
    {
        AuthenticationOrganisationSelectorExtensions.SetupSelectOrganisationFeatures(null!);
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_RegistersGeneralFeatures()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationFeatures();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ISelectOrganisationCallbackProcessor)
            )
        );
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_RegistersAuthenticationOrganisationSelector()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationFeatures();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IAuthenticationOrganisationSelector)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IOrganisationClaimManager)
            )
        );
    }

    #endregion
}
