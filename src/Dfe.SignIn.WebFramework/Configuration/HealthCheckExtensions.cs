using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Extension methods for setting up health checks in web applications.
/// </summary>
/// <remarks>
///   <para>These extensions are suitable for both MVC and minimal API applications.</para>
/// </remarks>
[ExcludeFromCodeCoverage]
public static class HealthCheckExtensions
{
    /// <summary>
    /// Expose the healthcheck via an endpoint.
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
