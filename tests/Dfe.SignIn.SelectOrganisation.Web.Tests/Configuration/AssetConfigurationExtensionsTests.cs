using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.SelectOrganisation.Web.Tests.Configuration;

[TestClass]
public sealed class AssetConfigurationExtensionsTests
{
    #region SetupFrontendAssets(IServiceCollection, Action<AssetOptions>)

    [TestMethod]
    public void SetupFrontendAssets_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => AssetConfigurationExtensions.SetupFrontendAssets(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
