using Dfe.SignIn.Core.Contracts.Messaging;
using Dfe.SignIn.Gateways.BullMq;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Provides extension methods for configuring messaging adapters in the service collection.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Adds messaging adapters to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the messaging adapters to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMessagingServices(this IServiceCollection services)
    {
        services.AddOptions<MessagingSettings>()
            .BindConfiguration(MessagingSettings.SectionName)
            .ValidateDataAnnotations();

        services.AddBullMqServices();

        return services;
    }
}
