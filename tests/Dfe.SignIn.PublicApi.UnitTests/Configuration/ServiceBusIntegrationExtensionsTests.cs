using Azure.Core;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class ServiceBusIntegrationExtensionsTests
{
    #region General configuration

    [TestMethod]
    public void AddServiceBusIntegration_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();
        var tokenCredential = new Mock<TokenCredential>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusIntegrationExtensions.AddServiceBusIntegration(null!, configuration, tokenCredential));
    }

    [TestMethod]
    public void AddServiceBusIntegration_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, null!, tokenCredential));
    }

    [TestMethod]
    public void AddServiceBusIntegration_Throws_WhenTokenCredentialArgumentIsNull()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, null!));
    }

    [TestMethod]
    public void AddServiceBusIntegration_DoesNothing_WhenConfigurationSectionMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var tokenCredential = new Mock<TokenCredential>().Object;

        ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, tokenCredential);

        Assert.IsFalse(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(ServiceBusClient)
            )
        );
    }

    [TestMethod]
    public void AddServiceBusIntegration_AddsIntegration()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ServiceBus:Namespace"] = "fake-fully-qualified-namespace",
            })
            .Build();

        ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, tokenCredential);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(ServiceBusClient)
            )
        );
        Assert.IsFalse(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
            )
        );
    }

    #endregion

    #region Application Message Topic

    [TestMethod]
    public void ApplicationMessageTopic_NoProcessor_WhenConfigurationMissing()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ServiceBus:Namespace"] = "fake-fully-qualified-namespace",
            })
            .Build();

        ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, tokenCredential);

        Assert.IsFalse(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
            )
        );
    }

    [TestMethod]
    public void ApplicationMessageTopic_AddsProcessor()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ServiceBus:Namespace"] = "fake-fully-qualified-namespace",
                ["ServiceBus:ApplicationsTopic:TopicName"] = "fake-topic-name",
                ["ServiceBus:ApplicationsTopic:SubscriptionName"] = "fake-subscription-name",
            })
            .Build();

        ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, tokenCredential);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
            )
        );
    }

    [TestMethod]
    public void ApplicationMessageTopic_AddsHealthCheck()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ServiceBus:Namespace"] = "fake-fully-qualified-namespace",
                ["ServiceBus:ApplicationsTopic:TopicName"] = "fake-topic-name",
                ["ServiceBus:ApplicationsTopic:SubscriptionName"] = "fake-subscription-name",
            })
            .Build();

        ServiceBusIntegrationExtensions.AddServiceBusIntegration(services, configuration, tokenCredential);

        var provider = services.BuildServiceProvider();
        var healthCheckOptions = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        Assert.IsTrue(
            healthCheckOptions.Value.Registrations.Any(
                reg => reg.Name == "topic:fake-topic-name"
            )
        );
    }

    #endregion
}
