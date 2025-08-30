using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class AssetConfigurationExtensionsTests
{
    #region SetupFrontendAssets(IServiceCollection, Action<AssetOptions>)

    [TestMethod]
    public void SetupFrontendAssets_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => AssetConfigurationExtensions.SetupFrontendAssets(
                services: null!
            ));
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
