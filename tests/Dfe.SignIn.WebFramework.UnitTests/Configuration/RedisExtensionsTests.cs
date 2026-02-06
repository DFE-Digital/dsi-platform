using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class RedisExtensionsTests
{
    #region SetupRedisCacheStore(IServiceCollection, IConfiguration)

    private static IConfiguration CreateFakeRedisConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("ConnectionString", "fake-connection-string"),
            ])
            .Build();
    }

    [TestMethod]
    public void SetupRedisCacheStore_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = CreateFakeRedisConfiguration();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisCacheStore(null!, "TestCacheKey", configuration));
    }

    [TestMethod]
    public void SetupRedisCacheStore_Throws_WhencCacheStoreKeyArgumentIsNull()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeRedisConfiguration();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisCacheStore(services, null!, configuration));
    }

    [TestMethod]
    public void SetupRedisCacheStore_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => RedisExtensions.SetupRedisCacheStore(services, "TestCacheKey", null!));
    }

    [TestMethod]
    public void SetupRedisCacheStore_Throws_WhenConnectionStringIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(()
            => RedisExtensions.SetupRedisCacheStore(services, "TestCacheKey", configuration));
        Assert.AreEqual("Missing connection string for Redis.", exception.Message);
    }

    [TestMethod]
    public void SetupRedisCacheStore_HasExpectedServices()
    {
        var configuration = CreateFakeRedisConfiguration();
        var services = new ServiceCollection();

        RedisExtensions.SetupRedisCacheStore(services, "TestCacheKey", configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(TimeProvider)
            )
        );
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
