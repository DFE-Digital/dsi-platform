using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceBusExtensionsTests
{
    #region AddServiceBusTopicProcessor(IConfigurationRoot, string)

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_ConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(null!, "TestTopic"));
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_SectionNameArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(configuration, null!));
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_SectionNameArgumentIsEmpty()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(configuration, ""));
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_ReturnsNull_WhenSectionNotPresent()
    {
        var configuration = new ConfigurationBuilder().Build();

        var result = ServiceBusExtensions.GetServiceBusTopicOptions(configuration, "TestTopic");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_ReturnsExpectedOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["TestTopic:TopicName"] = "test_topic",
                ["TestTopic:SubscriptionName"] = "test_subscription",
            })
            .Build();

        var result = ServiceBusExtensions.GetServiceBusTopicOptions(configuration, "TestTopic");

        Assert.IsNotNull(result);
        Assert.AreEqual("test_topic", result.TopicName);
        Assert.AreEqual("test_subscription", result.SubscriptionName);
    }

    #endregion

    #region IServiceCollection AddServiceBusTopicProcessor(IServiceCollection, Action<ServiceBusTopicProcessorOptions>)

    private sealed class ExampleMessageHandler : IServiceBusMessageHandler
    {
        public Task HandleAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusTopicProcessor(null!, new() {
                SubscriptionName = "example-subscription",
                TopicName = "example-topic",
            }));
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_WhenOptionsArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusTopicProcessor(services, null!));
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_WhenTopicNameIsMissing()
    {
        var services = new ServiceCollection();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(()
            => ServiceBusExtensions.AddServiceBusTopicProcessor(services, new() {
                SubscriptionName = "example-subscription",
                TopicName = null!,
            }));
        Assert.AreEqual("Missing topic name for Service Bus subscription.", exception.Message);
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_Throws_WhenSubscriptionNameIsMissing()
    {
        var services = new ServiceCollection();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(()
            => ServiceBusExtensions.AddServiceBusTopicProcessor(services, new() {
                SubscriptionName = null!,
                TopicName = "example-topic",
            }));
        Assert.AreEqual("Missing subscription name for Service Bus subscription.", exception.Message);
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_RegistersProcessingService_WhenHasHandlerRegistrations()
    {
        var services = new ServiceCollection();

        ServiceBusExtensions.AddServiceBusTopicProcessor(services, new() {
            SubscriptionName = "example-subscription",
            TopicName = "example-topic",
        });

        Assert.IsTrue(services.Any(descriptor =>
            descriptor.Lifetime == ServiceLifetime.Singleton &&
            descriptor.ServiceType == typeof(IHostedService)
        ));
    }

    #endregion
}
