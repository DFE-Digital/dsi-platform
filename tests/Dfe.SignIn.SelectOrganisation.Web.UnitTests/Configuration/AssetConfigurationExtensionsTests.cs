using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.Configuration;

[TestClass]
public sealed class AssetConfigurationExtensionsTests
{
    #region SetupFrontendAssets(IServiceCollection, Action<AssetOptions>)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupFrontendAssets_Throws_WhenServicesArgumentIsNull()
    {
        AssetConfigurationExtensions.SetupFrontendAssets(
            services: null!
        );
    }

    [TestMethod]
    public void SetupFrontendAssets_RegistersGovUkFrontendServices()
    {
        var services = new ServiceCollection();

        services.SetupFrontendAssets();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType.Name == "IGovUkHtmlGenerator"
            )
        );
    }

    #endregion
}
