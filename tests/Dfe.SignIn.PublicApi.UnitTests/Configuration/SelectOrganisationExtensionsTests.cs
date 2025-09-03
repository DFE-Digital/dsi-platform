using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupRedisSessionStore(IServiceCollection, IConfiguration)

    [TestMethod]
    public void SetupRedisSessionStore_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationRoot([]);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => SelectOrganisationExtensions.SetupRedisSessionStore(null!, configuration));
    }

    [TestMethod]
    public void SetupRedisSessionStore_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => services.SetupRedisSessionStore(null!));
    }

    [TestMethod]
    public void SetupRedisSessionStore_HasExpectedServices()
    {
        var configuration = new ConfigurationRoot([]);
        var services = new ServiceCollection();

        services.SetupRedisSessionStore(configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                (string?)descriptor.ServiceKey == SelectOrganisationConstants.CacheStoreKey &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
    }

    #endregion

    #region SetupSelectOrganisationInteractions(IServiceCollection)

    [TestMethod]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SelectOrganisationExtensions.SetupSelectOrganisationInteractions(
                services: null!
            ));
    }

    [DataRow(
        typeof(IInteractor<CreateSelectOrganisationSessionRequest>),
        DisplayName = nameof(CreateSelectOrganisationSessionRequest)
    )]
    [DataRow(
        typeof(IInteractor<FilterOrganisationsForUserRequest>),
        DisplayName = nameof(FilterOrganisationsForUserRequest)
    )]
    [DataTestMethod]
    public void SetupSelectOrganisationInteractions_HasExpectedInteractionType(
        Type interactionType)
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationInteractions();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == interactionType
            )
        );
    }

    #endregion
}
