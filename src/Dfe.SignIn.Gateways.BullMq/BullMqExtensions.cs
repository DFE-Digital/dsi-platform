using Dfe.SignIn.Core.Interfaces.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Extension methods for setting up BullMQ services.
/// </summary>
public static class BullMqExtensions
{
    /// <summary>
    /// Adds BullMQ services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Registers this gateway as the sole <see cref="IEventPublisher"/> implementation.
    /// A later <c>AddSingleton&lt;IEventPublisher, ...&gt;</c> registration would replace it.
    /// </remarks>
    public static IServiceCollection AddBullMqServices(
        this IServiceCollection services)
    {
        services.AddOptions<BullMqSettings>()
            .BindConfiguration(BullMqSettings.SectionName)
            .PostConfigure(options => {
                if (string.IsNullOrWhiteSpace(options.ConnectionString)) {
                    throw new InvalidOperationException("Redis connection string is not configured.");
                }

                // Append the Database Index to the connection string automatically
                var redisConfig = StackExchange.Redis.ConfigurationOptions.Parse(options.ConnectionString);
                redisConfig.DefaultDatabase = options.DatabaseIndex;
                options.ConnectionString = redisConfig.ToString(includePassword: true);
            });

        services.AddSingleton<IBullMqQueueFactory, BullMqQueueFactory>();
        services.AddSingleton<IEventPublisher, BullMqEventPublisher>();

        return services;
    }
}
