using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class InteractionCachingExtensionsTests
{
    #region AddInteractionCaching(IServiceCollection, IConfigurationRoot)

    [TestMethod]
    public void AddInteractionCaching_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionCachingExtensions.AddInteractionCaching(null!, configuration));
    }

    [TestMethod]
    public void AddInteractionCaching_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionCachingExtensions.AddInteractionCaching(services, null!));
    }

    [TestMethod]
    public void AddInteractionCaching_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = InteractionCachingExtensions.AddInteractionCaching(services, configuration);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddInteractionCaching_DoesNotSetupRedis_WhenConfigurationIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        InteractionCachingExtensions.AddInteractionCaching(services, configuration);

        Assert.IsFalse(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
    }

    private static void AddFakeInteractors(ServiceCollection services)
    {
        services.AddTransient(_
            => new Mock<IInteractor<GetApplicationApiConfigurationRequest>>().Object);
    }

    private static IConfigurationRoot GetFakeConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("InteractionsRedisCache:ConnectionString", "fake-connection-string"),
            ])
            .Build();
    }

    [TestMethod]
    public void AddInteractionCaching_SetupRedis()
    {
        var services = new ServiceCollection();
        AddFakeInteractors(services);

        var configuration = GetFakeConfiguration();

        InteractionCachingExtensions.AddInteractionCaching(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
    }

    #endregion
}
