using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceBusHandlerResolverTests
{
    private sealed class ExampleMessageHandler : IServiceBusMessageHandler
    {
        public Task HandleAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("unknown")]
    public void ResolveHandlers_ReturnsEmptyCollection_WhenSubjectCannotBeResolved(string subject)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var handlerRegistrations = new Dictionary<string, Type>();
        var resolver = new ServiceBusHandlerResolver(serviceProvider, "example_topic");

        var resolvedHandlers = resolver.ResolveHandlers(subject);

        Assert.IsEmpty(resolvedHandlers);
    }

    [TestMethod]
    public void ResolveHandlers_ResolvesExpectedHandler()
    {
        var services = new ServiceCollection();
        services.AddKeyedTransient<IServiceBusMessageHandler, ExampleMessageHandler>("example_topic:subject1");
        var serviceProvider = services.BuildServiceProvider();

        var resolver = new ServiceBusHandlerResolver(serviceProvider, "example_topic");

        var handlers = resolver.ResolveHandlers("subject1");
        Assert.HasCount(1, handlers);
        Assert.IsInstanceOfType<ExampleMessageHandler>(handlers.First());
    }
}
