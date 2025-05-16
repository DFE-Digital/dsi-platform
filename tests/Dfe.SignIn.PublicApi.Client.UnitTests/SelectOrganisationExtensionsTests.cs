using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupSelectOrganisationFeatures(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationFeatures_Throws_WhenServicesArgumentIsNull()
    {
        SelectOrganisationExtensions.SetupSelectOrganisationFeatures(null!);
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_ReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.SetupSelectOrganisationFeatures();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_RegistersGeneralFeatures()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationFeatures();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(ISelectOrganisationUserFlow)
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
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(ISelectOrganisationEvents)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(StandardSelectOrganisationMiddleware)
            )
        );
    }

    #endregion
}
