using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify;

/// <summary>
/// Extension methods for setting up GOV Notify.
/// </summary>
public static class GovNotifyExtensions
{
    /// <summary>
    /// Adds GOV Notify services.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddGovNotify(this IServiceCollection services, IConfiguration configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.Configure<GovNotifyOptions>(opts => configuration.GetSection(GovNotifyOptions.SectionName).Bind(opts));

        services.AddTransient<IAsyncNotificationClient, NotificationClient>(provider => {
            var optionsAccessor = provider.GetRequiredService<IOptions<GovNotifyOptions>>();
            return new NotificationClient(optionsAccessor.Value.ApiKey);
        });

        services.AddScoped<INotificationService, EmailNotificationServiceWithGovNotify>();

        return services;
    }
}
