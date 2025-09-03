using Dfe.SignIn.Base.Framework;
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
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddGovNotify(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddTransient<IAsyncNotificationClient, NotificationClient>(provider => {
            var optionsAccessor = provider.GetRequiredService<IOptions<GovNotifyOptions>>();
            return new NotificationClient(optionsAccessor.Value.ApiKey);
        });

        return services;
    }
}
