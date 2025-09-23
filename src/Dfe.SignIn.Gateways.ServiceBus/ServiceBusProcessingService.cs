using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// A hosted service that manages the lifecycle of an Azure Service Bus processor.
/// It starts and stops message processing when the application starts and stops.
/// </summary>
public sealed class ServiceBusProcessingService(
    ILogger<ServiceBusProcessingService> logger,
    ServiceBusProcessor processor,
    IServiceBusHandlerResolver handlerResolver
) : IHostedService
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage(
        Justification = "Service Bus events cannot be triggered by unit test."
    )]
    public Task StartAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync += this.OnProcessMessageAsync;
        processor.ProcessErrorAsync += this.OnProcessErrorAsync;
        return processor.StartProcessingAsync(cancellationToken);
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage(
        Justification = "Service Bus events cannot be triggered by unit test."
    )]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync -= this.OnProcessMessageAsync;
        processor.ProcessErrorAsync -= this.OnProcessErrorAsync;
        return processor.StopProcessingAsync(cancellationToken);
    }

    internal async Task OnProcessMessageAsync(ProcessMessageEventArgs e)
    {
        var handlers = handlerResolver.ResolveHandlers(e.Message.Subject);
        foreach (var handler in handlers) {
            if (e.CancellationToken.IsCancellationRequested) {
                break;
            }
            await handler.HandleAsync(e.Message, e.CancellationToken);
        }
    }

    internal Task OnProcessErrorAsync(ProcessErrorEventArgs e)
    {
        logger.LogError(e.Exception,
            "Service Bus error in entity {EntityPath}, operation {ErrorSource}",
            e.EntityPath, e.ErrorSource
        );
        return Task.CompletedTask;
    }
}
