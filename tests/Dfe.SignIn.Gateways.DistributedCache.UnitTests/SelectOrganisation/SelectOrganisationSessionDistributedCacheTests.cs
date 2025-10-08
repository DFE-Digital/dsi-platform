using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationSessionExtensionsTests
{
    #region AddSelectOrganisationSessionCache(IServiceCollection, Action<SelectOrganisationSessionCacheOptions>)

    [TestMethod]
    public void AddSelectOrganisationSessionCache_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SelectOrganisationSessionExtensions.AddSelectOrganisationSessionCache(
                services: null!
            ));
    }

    [TestMethod]
    public void AddSelectOrganisationSessionCache_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddSelectOrganisationSessionCache();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ICacheEntrySerializer) &&
                descriptor.ImplementationType == typeof(DefaultCacheEntrySerializer)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ISelectOrganisationSessionRepository) &&
                descriptor.ImplementationType == typeof(SelectOrganisationSessionDistributedCache)
            )
        );
    }

    #endregion
}
