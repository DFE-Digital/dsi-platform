using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Gateways.ServiceBus.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceBusExtensionsTests
{
    #region GetServiceBusTopicOptions(IConfigurationRoot, string)

    [TestMethod]
    public void GetServiceBusTopicOptions_Throws_ConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(null!, "TestTopic"));
    }

    [TestMethod]
    public void GetServiceBusTopicOptions_Throws_SectionNameArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(configuration, null!));
    }

    [TestMethod]
    public void GetServiceBusTopicOptions_Throws_SectionNameArgumentIsEmpty()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentException>(()
            => ServiceBusExtensions.GetServiceBusTopicOptions(configuration, ""));
    }

    [TestMethod]
    public void GetServiceBusTopicOptions_ReturnsNull_WhenSectionNotPresent()
    {
        var configuration = new ConfigurationBuilder().Build();

        var result = ServiceBusExtensions.GetServiceBusTopicOptions(configuration, "TestTopic");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetServiceBusTopicOptions_ReturnsExpectedOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("TestTopic:TopicName", "test_topic"),
                new("TestTopic:SubscriptionName", "test_subscription"),
            ])
            .Build();

        var result = ServiceBusExtensions.GetServiceBusTopicOptions(configuration, "TestTopic");

        Assert.IsNotNull(result);
        Assert.AreEqual("test_topic", result.TopicName);
        Assert.AreEqual("test_subscription", result.SubscriptionName);
    }

    #endregion

    #region AddServiceBusTopicProcessor(IServiceCollection, Action<ServiceBusTopicProcessorOptions>)

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
    public void AddServiceBusTopicProcessor_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = ServiceBusExtensions.AddServiceBusTopicProcessor(services, new() {
            SubscriptionName = "example-subscription",
            TopicName = "example-topic",
        });

        Assert.AreSame(result, services);
    }

    [TestMethod]
    public void AddServiceBusTopicProcessor_AddsRequiredServices_WhenHasHandlerRegistrations()
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

    #region AddServiceBusCacheInvalidator<TRequest>(IServiceCollection, string, string, CacheKeyFromServiceBusMessageDelegate)

    public sealed record FakeRequest : IKeyedRequest
    {
        public string Key => KeyedRequestConstants.DefaultKey;
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(null!, "topicName", "subject", _ => "key"));
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_Throws_WhenTopicOrQueueNameArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(services, null!, "subject", _ => "key"));
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_Throws_WhenSubjectArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(services, "topicName", null!, _ => "key"));
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_Throws_WhenGetCacheKeyArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(services, "topicName", "subject", null!));
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<IInteractionCache<FakeRequest>>().Object);

        var result = ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(services, "topicName", "subject", _ => "key");

        Assert.AreSame(result, services);
    }

    [TestMethod]
    public void AddServiceBusCacheInvalidator_AddsRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<IInteractionCache<FakeRequest>>().Object);

        ServiceBusExtensions.AddServiceBusCacheInvalidator<FakeRequest>(services, "topicName", "subject", _ => "key");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredKeyedService<IServiceBusMessageHandler>("topicName:subject");

        Assert.IsInstanceOfType<ServiceBusCacheInvalidationHandler<FakeRequest>>(handler);
    }

    #endregion

    #region AddAuditingWithServiceBus(IServiceCollection, IConfigurationRoot)

    [TestMethod]
    public void AddAuditingWithServiceBus_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddAuditingWithServiceBus(null!, configuration));
    }

    [TestMethod]
    public void AddAuditingWithServiceBus_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddAuditingWithServiceBus(services, null!));
    }

    [TestMethod]
    public void AddAuditingWithServiceBus_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("ServiceBus:AuditTopic:TopicName", "test_topic"),
            ])
            .Build();

        var result = ServiceBusExtensions.AddAuditingWithServiceBus(services, configuration);

        Assert.AreSame(result, services);
    }

    [TestMethod]
    public void AddAuditingWithServiceBus_Throws_WhenMissingTopicOptions()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(()
            => ServiceBusExtensions.AddAuditingWithServiceBus(services, configuration));
        Assert.AreEqual("Missing topic options.", exception.Message);
    }

    [TestMethod]
    public void AddAuditingWithServiceBus_AddsRequiredServices()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("ServiceBus:AuditTopic:TopicName", "test_topic"),
            ])
            .Build();

        ServiceBusExtensions.AddAuditingWithServiceBus(services, configuration);

        Assert.IsTrue(services.Any(descriptor =>
            (string?)descriptor.ServiceKey == ServiceBusExtensions.AuditSenderKey &&
            descriptor.Lifetime == ServiceLifetime.Singleton &&
            descriptor.ServiceType == typeof(ServiceBusSender)
        ));
        Assert.IsTrue(services.Any(descriptor =>
            descriptor.Lifetime == ServiceLifetime.Transient &&
            descriptor.ServiceType == typeof(IInteractor<WriteToAuditRequest>) &&
            descriptor.ImplementationType == typeof(WriteToAuditWithServiceBus)
        ));
    }

    #endregion
}
