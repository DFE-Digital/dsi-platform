using Azure.Messaging.ServiceBus;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Options for a Service Bus topic processor.
/// </summary>
public sealed class ServiceBusTopicProcessorOptions : ServiceBusProcessorOptions
{
    /// <summary>
    /// Gets or sets the name of the topic.
    /// </summary>
    public required string TopicName { get; set; }

    /// <summary>
    /// Gets or sets the name of the topic subscription.
    /// </summary>
    public required string SubscriptionName { get; set; }
}
