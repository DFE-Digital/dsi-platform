using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.SelectOrganisation.SessionData.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.SessionData.UnitTests;

[TestClass]
public sealed class SelectOrganisationSessionCacheExtensionsTests
{
    #region AddSelectOrganisationSessionCache(IServiceCollection, Action<SelectOrganisationSessionCacheOptions>)

    [TestMethod]
    public void AddSelectOrganisationSessionCache_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => SelectOrganisationSessionCacheExtensions.AddSelectOrganisationSessionCache(null, (options) => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddSelectOrganisationSessionCache_Throws_WhenOptionsArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddSelectOrganisationSessionCache(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddSelectOrganisationSessionCache_ConfigureUsingSetupAction()
    {
        var services = new ServiceCollection();

        services.AddSelectOrganisationSessionCache(options =>
        {
            options.CacheKeyPrefix = "overriden-prefix:";
        });

        var factory = new DefaultServiceProviderFactory();
        var provider = factory.CreateServiceProvider(services);
        var actualOptions = provider.GetRequiredService<IOptions<SelectOrganisationSessionCacheOptions>>();

        Assert.AreEqual("overriden-prefix:", actualOptions?.Value.CacheKeyPrefix);
    }

    [TestMethod]
    public void AddSelectOrganisationSessionCache_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddSelectOrganisationSessionCache(options => { });

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ISessionDataSerializer) &&
                descriptor.ImplementationType == typeof(DefaultSessionDataSerializer)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ISelectOrganisationSessionRepository) &&
                descriptor.ImplementationType == typeof(DistributedCacheSelectOrganisationSessionRepository)
            )
        );
    }

    #endregion
}
