using Dfe.SignIn.WebFramework.Configuration;
using GovUk.Frontend.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

    [TestMethod]
    public void SetupFrontendAssets_SetsRebrandOptionToTrue()
    {
        var services = new ServiceCollection();

        services.SetupFrontendAssets();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<GovUkFrontendOptions>>();

        var rebrandValue = options.Value.Rebrand;

        Assert.IsTrue(rebrandValue);
    }

    #endregion
}
