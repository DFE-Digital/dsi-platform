using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.Configuration;

[TestClass]
public sealed class HealthCheckExtensionsTests
{
    #region SetupHealthChecks(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupHealthChecks_Throws_WhenServiceArgumentIsNull()
    {
        var configuration = new ConfigurationRoot([]);

        HealthCheckExtensions.SetupHealthChecks(
            services: null!,
            configuration: configuration
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupHealthChecks_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        HealthCheckExtensions.SetupHealthChecks(
            services: services,
            configuration: null!
        );
    }

    [TestMethod]
    public void SetupHealthChecks_AddsHealthChecksService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationRoot([]);

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
        var configuration = new ConfigurationRoot([]);

        services.SetupHealthChecks(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registrations = options.Value.Registrations;

        var redisCheck = registrations.FirstOrDefault(r => r.Name == "redis");
        Assert.IsNotNull(redisCheck);
    }
    #endregion
}
