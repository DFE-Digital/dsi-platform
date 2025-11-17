using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;
using Dfe.SignIn.Gateways.ServiceBus.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Extension methods for setting up Service Bus processing.
/// </summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Gets any configuration that has been associated with a Service Bus topic.
    /// </summary>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <returns>
    ///   <para>An instance of the <see cref="ServiceBusTopicProcessorOptions"/>
    ///   class when the configuration section was present.</para>
    ///   <para>- or -</para>
    ///   <para>A value of null indicating that the configuration section was
    ///   not present.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="configuration"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="sectionName"/> is null or empty.</para>
    /// </exception>
    public static ServiceBusTopicProcessorOptions? GetServiceBusTopicOptions(
        this IConfigurationRoot configuration, string sectionName)
    {
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(sectionName, nameof(sectionName));

        var section = configuration.GetSection(sectionName);
        if (!section.Exists()) {
            return null;
        }

        var options = Activator.CreateInstance<ServiceBusTopicProcessorOptions>();
        section.Bind(options);
        return options;
    }

    /// <summary>
    /// Adds a service for processing messages from a Service Bus topic subscription.
    /// </summary>
    /// <remarks>
    ///   <para>A Service Bus processor is only setup as a hosted service when at least
    ///   one service bus handler has been specified.</para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Options for the topic subscription.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="options"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If topic name was not specified.</para>
    ///   <para>- or -</para>
    ///   <para>If subscription name was not specified.</para>
    /// </exception>
    public static IServiceCollection AddServiceBusTopicProcessor(
        this IServiceCollection services, ServiceBusTopicProcessorOptions options)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(options, nameof(options));

        if (string.IsNullOrWhiteSpace(options.TopicName)) {
            throw new InvalidOperationException("Missing topic name for Service Bus subscription.");
        }
        if (string.IsNullOrWhiteSpace(options.SubscriptionName)) {
            throw new InvalidOperationException("Missing subscription name for Service Bus subscription.");
        }

        services.AddSingleton<IHostedService>(provider => {
            var client = provider.GetRequiredService<ServiceBusClient>();
            var processor = client.CreateProcessor(options.TopicName, options.SubscriptionName, options);
            var resolver = new ServiceBusHandlerResolver(provider, options.TopicName);
            return ActivatorUtilities.CreateInstance<ServiceBusProcessingService>(provider, processor, resolver);
        });

        return services;
    }

    /// <summary>
    /// Registers a Service Bus processing handler for topic message events.
    /// </summary>
    /// <typeparam name="THandler">The type of handler to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="topicOrQueueName">Name of the Service Bus topic or queue.</param>
    /// <param name="subject">The subject that uniquely identifies the message type.</param>
    /// <param name="parameters">Any additional parameters to provide to handler constructors.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topicOrQueueName"/> is null or empty.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="subject"/> is null or empty.</para>
    /// </exception>
    public static IServiceCollection AddServiceBusHandler<THandler>(
        this IServiceCollection services, string topicOrQueueName, string subject, params object[] parameters)
        where THandler : class, IServiceBusMessageHandler
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(topicOrQueueName, nameof(topicOrQueueName));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(subject, nameof(subject));

        string key = $"{topicOrQueueName}:{subject}";

        return services.AddKeyedTransient<IServiceBusMessageHandler>(key, (provider, _)
            => ActivatorUtilities.CreateInstance<THandler>(provider, parameters));
    }

    /// <summary>
    /// Adds cache invalidation handler for interaction requests of a given type.
    /// </summary>
    /// <typeparam name="TRequest">The type of interaction request.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="topicOrQueueName">Name of the Service Bus topic or queue.</param>
    /// <param name="subject">The subject that uniquely identifies the message type.</param>
    /// <param name="getCacheKey">A delegate that determines a cache key from a Service Bus message.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="topicOrQueueName"/> is null or empty.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="subject"/> is null or empty.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="getCacheKey"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddServiceBusCacheInvalidator<TRequest>(
        this IServiceCollection services, string topicOrQueueName, string subject, CacheKeyFromServiceBusMessageDelegate getCacheKey)
        where TRequest : class, ICacheableRequest
    {
        ExceptionHelpers.ThrowIfArgumentNull(getCacheKey, nameof(getCacheKey));

        return AddServiceBusHandler<ServiceBusCacheInvalidationHandler<TRequest>>(
            services, topicOrQueueName, subject, getCacheKey);
    }

    /// <summary>
    /// Identifies the keyed <see cref="ServiceBusSender"/> singleton for sending messages
    /// to the audit topic in Service Bus.
    /// </summary>
    public const string AuditSenderKey = "161b6895-e195-4fed-b102-6dcf8bf0cd75";

    /// <summary>
    /// Adds auditing capabilities.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If topic name was not specified.</para>
    /// </exception>
    public static IServiceCollection AddAuditingWithServiceBus(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        var options = configuration.GetServiceBusTopicOptions("ServiceBus:AuditTopic")
            ?? throw new InvalidOperationException("Missing topic options.");

        services.AddKeyedSingleton(AuditSenderKey, (provider, _) => {
            var client = provider.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(options.TopicName);
        });

        services.AddInteractor<WriteToAuditWithServiceBus>();

        return services;
    }
}
