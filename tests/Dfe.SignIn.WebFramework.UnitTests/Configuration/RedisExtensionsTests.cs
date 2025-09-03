using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class RedisExtensionsTests
{
    #region SetupRedisSessionStore(IServiceCollection, IConfiguration)

    [TestMethod]
    public void SetupRedisSessionStore_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationRoot([]);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisSessionStore(
                null!, "TestCacheKey", configuration));
    }

    [TestMethod]
    public void SetupRedisSessionStore_Throws_WhencCacheStoreKeyArgumentIsNull()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationRoot([]);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisSessionStore(
                services, null!, configuration));
    }

    [TestMethod]
    public void SetupRedisSessionStore_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisSessionStore(
                services, "TestCacheKey", null!));
    }

    [TestMethod]
    public void SetupRedisSessionStore_HasExpectedServices()
    {
        var configuration = new ConfigurationRoot([]);
        var services = new ServiceCollection();

        RedisExtensions.SetupRedisSessionStore(
            services, "TestCacheKey", configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                (string?)descriptor.ServiceKey == "TestCacheKey" &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
    }

    #endregion
}
