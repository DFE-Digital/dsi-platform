using Azure.Messaging.ServiceBus;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Represents a handler for processing message events received from Azure Service Bus.
/// </summary>
public interface IServiceBusMessageHandler
{
    /// <summary>
    /// Handles an incoming Service Bus message.
    /// </summary>
    /// <param name="message">The message that is being processed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="OperationCanceledException" />
    Task HandleAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default);
}
