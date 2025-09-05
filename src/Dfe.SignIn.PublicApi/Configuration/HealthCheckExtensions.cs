using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up health checks.
/// </summary>
[ExcludeFromCodeCoverage]
public static class HealthCheckExtensions
{
    /// <summary>
    /// Setup and register the healthcheck service.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static void SetupHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        string connectionString = configuration.GetValue<string>("ConnectionString")
            ?? throw new InvalidOperationException("Missing connection string for Redis.");

        services.AddHealthChecks()
            .AddRedis(
                redisConnectionString: connectionString,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                timeout: TimeSpan.FromSeconds(5)
            );
    }
}
