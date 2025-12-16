using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Provides extension methods for configuring user sessions in MVC applications.
/// </summary>
public static class UserSessionExtensions
{
    /// <summary>
    /// The name of the redis configuration section for sessions.
    /// </summary>
    public const string SessionRedisCacheConfigurationSectionName = "SessionRedisCache";

    /// <summary>
    /// The name of the configuration section for sessions.
    /// </summary>
    public const string SessionConfigurationSectionName = "Session";

    /// <summary>
    /// The name of the session cookie.
    /// </summary>
    public const string SessionCookieName = "Session";

    /// <summary>
    /// Adds the fundemental user session configuration.
    /// This is required for automatic session timeouts as well as persisted sessions.
    /// </summary>
    /// <param name="services">The collection of services to add to.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>The service collection instance for chaining.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddUserSessions(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.Configure<UserSessionOptions>(
            configuration.GetRequiredSection(SessionConfigurationSectionName));

        return services;
    }

    /// <summary>
    /// Adds session storage using the distributed cache that is associated with
    /// the service key <see cref="DistributedCacheKeys.SessionCache"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Ensure that <see cref="AddUserSessions"/> is also used to provide
    ///   the required user session configuration.</para>
    /// </remarks>
    /// <param name="services">The collection of services to add to.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>The service collection instance for chaining.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddSessionStorageWithDistributedCache(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.SetupRedisCacheStore(DistributedCacheKeys.SessionCache,
            configuration.GetRequiredSection(SessionRedisCacheConfigurationSectionName));

        services.AddSingleton(provider => provider.GetRequiredKeyedService<IDistributedCache>(
            DistributedCacheKeys.SessionCache));

        services.AddOptions<SessionOptions>().Configure<IOptions<UserSessionOptions>>((options, sessionOptions) => {
            options.Cookie.Name = SessionCookieName;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromMinutes(sessionOptions.Value.DurationInMinutes);
        });

        services.AddSession();

        return services;
    }
}
