using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Represents a service that can resolve a handler for a Service Bus message.
/// </summary>
public interface IServiceBusHandlerResolver
{
    /// <summary>
    /// Resolves a handler for a Service Bus message for a given subject.
    /// </summary>
    /// <param name="subject">The subject of the message.</param>
    /// <returns>
    ///   <para>An enumerable collection of zero-or-more <see cref="IServiceBusMessageHandler"/>
    ///   instances of which are able to handle messages with the given subject.</para>
    /// </returns>
    IEnumerable<IServiceBusMessageHandler> ResolveHandlers(string subject);
}

/// <summary>
/// A concrete implementation of <see cref="IServiceBusHandlerResolver"/> which resolves
/// <see cref="IServiceBusMessageHandler"/> instances from a <see cref="IServiceProvider"/>.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="groupKey">A base key representing a group of topic/queue handlers.</param>
public sealed class ServiceBusHandlerResolver(IServiceProvider serviceProvider, string groupKey)
    : IServiceBusHandlerResolver
{
    /// <inheritdoc/>
    public IEnumerable<IServiceBusMessageHandler> ResolveHandlers(string subject)
    {
        return !string.IsNullOrWhiteSpace(subject)
            ? serviceProvider.GetKeyedServices<IServiceBusMessageHandler>($"{groupKey}:{subject}")
            : [];
    }
}
