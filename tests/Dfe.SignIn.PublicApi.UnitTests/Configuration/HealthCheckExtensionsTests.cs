using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class HealthCheckExtensionsTests
{
    #region SetupHealthChecks(IServiceCollection)

    [TestMethod]
    public void SetupHealthChecks_Throws_WhenServiceArgumentIsNull()
    {
        var configuration = new ConfigurationRoot([]);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => HealthCheckExtensions.SetupHealthChecks(
                services: null!,
                configuration: configuration
            ));
    }

    [TestMethod]
    public void SetupHealthChecks_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => HealthCheckExtensions.SetupHealthChecks(
                services: services,
                configuration: null!
            ));
    }

    [TestMethod]
    public void SetupHealthChecks_AddsHealthChecksService()
    {
        var services = new ServiceCollection();
        var configuration = GetFakeConfiguration();

        services.SetupHealthChecks(configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(HealthCheckService)
            )
        );
    }

    [TestMethod]
    public void SetupHealthChecks_AddsRedisHealthCheckService()
    {
        var services = new ServiceCollection();
        var configuration = GetFakeConfiguration();

        services.SetupHealthChecks(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registrations = options.Value.Registrations;

        var redisCheck = registrations.FirstOrDefault(r => r.Name == "redis");
        Assert.IsNotNull(redisCheck);
    }

    [TestMethod]
    public void SetupHealthChecks_Throws_WhenRedisConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationRoot([]);

        Assert.ThrowsExactly<InvalidOperationException>(()
            => services.SetupHealthChecks(configuration));
    }

    private static IConfiguration GetFakeConfiguration()
    {
        var configurationData = new Dictionary<string, string>
        {
            {"ConnectionString", "fakeRedisConnectionString"}
        };

        var services = new ServiceCollection();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();
    }
    #endregion
}
