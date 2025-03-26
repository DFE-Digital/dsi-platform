using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache.UnitTests;

[TestClass]
public sealed class SelectOrganisationSessionCacheExtensionsTests
{
    #region AddSelectOrganisationSessionCache(IServiceCollection, Action<SelectOrganisationSessionCacheOptions>)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddSelectOrganisationSessionCache_Throws_WhenServicesArgumentIsNull()
    {
        SelectOrganisationSessionCacheExtensions.AddSelectOrganisationSessionCache(
            services: null!
        );
    }

    [TestMethod]
    public void AddSelectOrganisationSessionCache_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddSelectOrganisationSessionCache();

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
