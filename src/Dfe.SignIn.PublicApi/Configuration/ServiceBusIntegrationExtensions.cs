using Azure.Core;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Gateways.ServiceBus;

namespace Dfe.SignIn.PublicApi.Configuration;

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

        AddServiceBusProcessors(services, configuration, options, tokenCredential);

        return services;
    }

    private static void AddServiceBusProcessors(
        IServiceCollection services, IConfigurationRoot configuration, ServiceBusOptions options, TokenCredential tokenCredential)
    {
        var applicationsTopicOptions = configuration.GetServiceBusTopicOptions("ServiceBus:ApplicationsTopic");
        if (applicationsTopicOptions is not null) {
            services.AddServiceBusTopicProcessor(applicationsTopicOptions);
            AddApplicationsTopicHandlers(services, applicationsTopicOptions);

            services.AddHealthChecks().AddAzureServiceBusTopic(
                fullyQualifiedNamespace: options.Namespace,
                topicName: applicationsTopicOptions.TopicName,
                tokenCredential,
                name: $"topic:{applicationsTopicOptions.TopicName}"
            );
        }
    }

    private static void AddApplicationsTopicHandlers(
        IServiceCollection services, ServiceBusTopicProcessorOptions topicOptions)
    {
        services.AddServiceBusCacheInvalidator<GetApplicationApiConfigurationRequest>(
            topicOptions.TopicName,
            ApplicationMessagingSubjects.ConfigurationUpdated,
            msg => msg.ApplicationProperties["ClientId"].ToString()
        );
    }
}
