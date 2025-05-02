using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <exclude/>
[ExcludeFromCodeCoverage]
public static class HealthCheckExtensions
{
    /// <summary>
    /// Setup and register the heathcheck service.
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
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

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

    /// <summary>
    /// Expose the heathcheck via an endpoint.
    /// </summary>
    /// <param name="builder">The builder to register the healthchecks on.</param>
    /// <param name="endpoint">The endpoint which healthchecks will be made available, defaulting to '/v2/healthcheck'.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="builder"/> is null.</para>
    /// </exception>
    public static void UseHealthChecks(this IApplicationBuilder builder, string endpoint = "/v2/healthcheck")
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.UseHealthChecks(endpoint, new HealthCheckOptions {
            Predicate = _ => true,
            ResponseWriter = async (context, report) => {
                var result = JsonSerializer.Serialize(new {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks = report.Entries.Select(e => new {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds,
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }
        });
    }
}
