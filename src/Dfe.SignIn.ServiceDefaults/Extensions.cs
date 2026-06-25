using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
/// This project should be referenced by each service project in your solution.
/// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Extension method to add common service defaults to the host builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="additionalExcludedTracePaths">Additional endpoints to exclude from OpenTelemetry tracing.</param>
    /// <returns>The builder instance to allow chaining.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, string[]? additionalExcludedTracePaths = null) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry(additionalExcludedTracePaths);

        // builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http => {
            // Turn on resilience by default
            // http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry logging, metrics, and tracing.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="additionalExcludedTracePaths">Additional endpoints to exclude from OpenTelemetry tracing.</param>
    /// <returns>The builder instance to allow chaining.</returns>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, string[]? additionalExcludedTracePaths = null) where TBuilder : IHostApplicationBuilder
    {
        string[] defaultExcludedPaths = ["/health", "/alive"];

        builder.Logging.AddOpenTelemetry(logging => {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing => {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context => {
                            foreach (var path in defaultExcludedPaths) {
                                if (context.Request.Path.StartsWithSegments(path)) {
                                    return false;
                                }
                            }

                            if (additionalExcludedTracePaths != null) {
                                foreach (var path in additionalExcludedTracePaths) {
                                    if (context.Request.Path.StartsWithSegments(path)) {
                                        return false;
                                    }
                                }
                            }

                            return true;
                        }
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter) {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Azure Monitor — only enabled when connection string is configured.
        // Sampling ratio controls what proportion of traces are sent to App Insights.
        // Errors and exceptions are always captured regardless of sampling ratio.
        if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])) {
            builder.Services.AddOpenTelemetry()
                .UseAzureMonitor(options => {
                    // Sampling ratio: 1.0 = 100% of traces (full visibility, higher cost)
                    //                  0.5 = 50%  of traces (balanced)
                    //                  0.25 = 25% of traces (cost-conscious)
                    //                  0.1 = 10%  of traces (minimal cost, high-traffic services)
                    //
                    // Override per environment via appsettings or environment variables.
                    // Defaults to 0.1 (10%) so costs remain low if config is not set.
                    // Errors/exceptions are NEVER sampled out regardless of this value.
                    options.SamplingRatio =
                        float.TryParse(builder.Configuration["AzureMonitor:SamplingRatio"], out var ratio)
                            ? ratio
                            : 0.1f;
                });
        }

        return builder;
    }
}
