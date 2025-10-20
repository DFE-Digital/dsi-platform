using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Configuration;

[TestClass]
public sealed class UserSessionExtensionsTests
{
    #region AddUserSessions(IServiceCollection, IConfigurationRoot)

    private static IConfigurationRoot CreateFakeConfigurationForAddUserSessions()
        => new ConfigurationBuilder()
            .AddInMemoryCollection([
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes", "25"),
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:NotifyRemainingMinutes", "2.5"),
            ])
            .Build();

    [TestMethod]
    public void AddUserSessions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => UserSessionExtensions.AddUserSessions(
                services: null!,
                configuration: new ConfigurationBuilder().Build()
            ));
    }

    [TestMethod]
    public void AddUserSessions_Throws_WhenConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => UserSessionExtensions.AddUserSessions(
                services: new ServiceCollection(),
                configuration: null!
            ));
    }

    [TestMethod]
    public void AddUserSessions_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddUserSessions();

        var result = UserSessionExtensions.AddUserSessions(services, configuration);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddUserSessions_SetsUserSessionOptions()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddUserSessions();

        UserSessionExtensions.AddUserSessions(services, configuration);

        var provider = services.BuildServiceProvider();
        var optionsAccessor = provider.GetRequiredService<IOptions<UserSessionOptions>>();
        var options = optionsAccessor.Value;

        Assert.AreEqual(25.0, options.DurationInMinutes);
        Assert.AreEqual(2.5, options.NotifyRemainingMinutes);
    }

    #endregion

    #region AddSessionStorageWithDistributedCache(IServiceCollection, IConfigurationRoot)

    private static IConfigurationRoot CreateFakeConfigurationForAddSessionStorageWithDistributedCache()
        => new ConfigurationBuilder()
            .AddInMemoryCollection([
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes", "25"),
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:NotifyRemainingMinutes", "2.5"),
            ])
            .AddInMemoryCollection([
                new($"{UserSessionExtensions.SessionRedisCacheConfigurationSectionName}:ConnectionString", "localhost"),
                new($"{UserSessionExtensions.SessionRedisCacheConfigurationSectionName}:DatabaseNumber", "1"),
            ])
            .Build();

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => UserSessionExtensions.AddSessionStorageWithDistributedCache(
                services: null!,
                configuration: new ConfigurationBuilder().Build()
            ));
    }

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_Throws_WhenConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => UserSessionExtensions.AddSessionStorageWithDistributedCache(
                services: new ServiceCollection(),
                configuration: null!
            ));
    }

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddSessionStorageWithDistributedCache();

        var result = UserSessionExtensions.AddSessionStorageWithDistributedCache(services, configuration);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_AddsExpectedRedisCache()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddSessionStorageWithDistributedCache();

        UserSessionExtensions.AddUserSessions(services, configuration);
        UserSessionExtensions.AddSessionStorageWithDistributedCache(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.IsKeyedService &&
                descriptor.ServiceKey!.Equals(DistributedCacheKeys.SessionCache) &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                !descriptor.IsKeyedService &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IDistributedCache)
            )
        );
    }

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_SetsSessionOptions()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddSessionStorageWithDistributedCache();

        UserSessionExtensions.AddUserSessions(services, configuration);
        UserSessionExtensions.AddSessionStorageWithDistributedCache(services, configuration);

        var provider = services.BuildServiceProvider();
        var optionsAccessor = provider.GetRequiredService<IOptions<SessionOptions>>();
        var options = optionsAccessor.Value;

        Assert.AreEqual(UserSessionExtensions.SessionCookieName, options.Cookie.Name);
        Assert.AreEqual(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.AreEqual(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.IsTrue(options.Cookie.HttpOnly);
        Assert.IsTrue(options.Cookie.IsEssential);
        Assert.AreEqual(25.0, options.IdleTimeout.TotalMinutes);
    }

    [TestMethod]
    public void AddSessionStorageWithDistributedCache_AddsSessionServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddSessionStorageWithDistributedCache();

        UserSessionExtensions.AddUserSessions(services, configuration);
        UserSessionExtensions.AddSessionStorageWithDistributedCache(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(ISessionStore)
            )
        );
    }

    #endregion
}
