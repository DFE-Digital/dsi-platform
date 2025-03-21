using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;
using Dfe.SignIn.PublicApi.Configuration.Interactions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration.Interactions;

[TestClass]
public sealed class SelectOrganisationExtensionsTests
{
    #region SetupRedisSessionStore(IServiceCollection, IConfiguration)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupRedisSessionStore_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationRoot([]);

        SelectOrganisationExtensions.SetupRedisSessionStore(null!, configuration);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupRedisSessionStore_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        services.SetupRedisSessionStore(null!);
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationInteractions_Throws_WhenServicesArgumentIsNull()
    {
        SelectOrganisationExtensions.SetupSelectOrganisationInteractions(
            services: null!
        );
    }

    [DataRow(
        typeof(IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>),
        DisplayName = nameof(CreateSelectOrganisationSessionRequest)
    )]
    [DataRow(
        typeof(IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse>),
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
