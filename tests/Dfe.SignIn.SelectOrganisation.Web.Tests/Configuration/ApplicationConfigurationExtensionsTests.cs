using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Tests.Configuration;

[TestClass]
public sealed class ApplicationConfigurationExtensionsTests
{
    #region AddApplication(IServiceCollection, Action<ApplicationOptions>)

    [TestMethod]
    public void AddApplication_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ApplicationConfigurationExtensions.AddApplication(null, (options) => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddApplication_Throws_WhenOptionsArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddApplication(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddApplication_ConfigureUsingSetupAction()
    {
        var services = new ServiceCollection();

        services.AddApplication(options => {
            options.ServicesUrl = new Uri("https://overriden-services.localhost");
        });

        var factory = new DefaultServiceProviderFactory();
        var provider = factory.CreateServiceProvider(services);
        var actualOptions = provider.GetRequiredService<IOptions<ApplicationOptions>>();

        Assert.AreEqual(new Uri("https://overriden-services.localhost"), actualOptions?.Value.ServicesUrl);
    }

    #endregion
}
