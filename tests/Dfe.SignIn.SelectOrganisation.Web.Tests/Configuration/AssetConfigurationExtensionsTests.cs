using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Tests.Configuration;

[TestClass]
public sealed class AssetConfigurationExtensionsTests
{
    #region AddFrontendAssets(IServiceCollection, Action<AssetOptions>)

    [TestMethod]
    public void AddFrontendAssets_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => AssetConfigurationExtensions.AddFrontendAssets(null, (options) => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddFrontendAssets_Throws_WhenOptionsArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddFrontendAssets(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddFrontendAssets_ConfigureUsingSetupAction()
    {
        var services = new ServiceCollection();

        services.AddFrontendAssets(options => {
            options.Version = "42";
        });

        var factory = new DefaultServiceProviderFactory();
        var provider = factory.CreateServiceProvider(services);
        var actualOptions = provider.GetRequiredService<IOptions<AssetOptions>>();

        Assert.AreEqual("42", actualOptions?.Value.Version);
    }

    [TestMethod]
    public void AddFrontendAssets_RegistersGovUkFrontendServices()
    {
        var services = new ServiceCollection();

        services.AddFrontendAssets(options => { });

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType.Name == "IGovUkHtmlGenerator"
            )
        );
    }

    #endregion
}
