using Azure.Core;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Fn.AuthExtensions.Configuration;

/// <summary>
/// Extension methods for setting up Service Bus integration.
/// </summary>
public static class ServiceBusIntegrationExtensions
{
    /// <summary>
    /// Adds Service Bus integration with expected topic processors.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="tokenCredential">Token credential to connect to Azure Service Bus.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="tokenCredential"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddServiceBusIntegration(
        this IServiceCollection services, IConfigurationRoot configuration, TokenCredential tokenCredential)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));
        ExceptionHelpers.ThrowIfArgumentNull(tokenCredential, nameof(tokenCredential));

        var serviceBusSection = configuration.GetSection("ServiceBus");
        if (!serviceBusSection.Exists()) {
            // Service Bus has not been configured; do nothing.
            return services;
        }

        var options = Activator.CreateInstance<ServiceBusOptions>();
        serviceBusSection.Bind(options);

        services.AddSingleton(provider => {
            return new ServiceBusClient(options.Namespace, tokenCredential, new() {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
            });
        });

        return services;
    }
}
