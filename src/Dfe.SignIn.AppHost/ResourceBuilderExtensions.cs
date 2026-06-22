using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dfe.SignIn.AppHost;

/// <summary>
/// Provides extension methods for configuring resources.
/// </summary>
public static class ResourceBuilderExtensions
{
    /// <summary>
    /// Configures a project resource with shared configuration settings from the application configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <param name="configuration">The application configuration containing the settings.</param>
    /// <param name="frontendEndpoint">The frontend endpoint reference for asset base address configuration.</param>
    /// <returns>The configured resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithSharedConfiguration(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration,
        EndpointReference frontendEndpoint)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Local";
        var platformConfig = configuration.GetSection("Platform");
        var securityHeaderConfig = configuration.GetSection("SecurityHeaderPolicy");
        var internalApiConfig = configuration.GetSection("InternalApiClient");
        var govNotifyConfig = configuration.GetSection("GovNotify");
        var supportEmailConfig = configuration.GetSection("RaiseSupportTicketByEmail");
        var oidcConfig = configuration.GetSection("Oidc");
        var externalIdConfig = configuration.GetSection("ExternalId");
        var sessionConfig = configuration.GetSection("Session");

        builder
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
            .WithEnvironment("Platform__HelpUrl", platformConfig["HelpUrl"])
            .WithEnvironment("Platform__ProfileUrl", platformConfig["ProfileUrl"])
            .WithEnvironment("Platform__SurveyUrl", platformConfig["SurveyUrl"])
            .WithEnvironment("Platform__ServicesUrl", platformConfig["ServicesUrl"])
            .WithEnvironment("InternalApiClient__BaseAddress", internalApiConfig["BaseAddress"])
            .WithEnvironment("InternalApiClient__ClientId", internalApiConfig["ClientId"])
            .WithEnvironment("InternalApiClient__ClientSecret", internalApiConfig["ClientSecret"])
            .WithEnvironment("InternalApiClient__HostUrl", internalApiConfig["HostUrl"])
            .WithEnvironment("InternalApiClient__Resource", internalApiConfig["Resource"])
            .WithEnvironment("InternalApiClient__Tenant", internalApiConfig["Tenant"])
            .WithEnvironment("InternalApiClient__ProxyUrl", internalApiConfig["ProxyUrl"])
            .WithEnvironment("InternalApiClient__UseProxy", internalApiConfig["UseProxy"])
            .WithEnvironment("InternalApiClient__Directories__BaseAddress", internalApiConfig["Directories:BaseAddress"])
            .WithEnvironment("InternalApiClient__Applications__BaseAddress", internalApiConfig["Applications:BaseAddress"])
            .WithEnvironment("Assets__BaseAddress", frontendEndpoint);

        return builder;
    }

    /// <summary>
    /// Adds a Node.js platform application resource with preconfigured HTTPS endpoint, environment variables, and
    /// optional Redis and frontend endpoint integration.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="nodeRootDir">The root directory containing Node.js applications.</param>
    /// <param name="appName">The name of the application subdirectory within the root directory.</param>
    /// <param name="port">The port number for the HTTPS endpoint.</param>
    /// <param name="redisConnectionString">Optional Redis connection string reference to be configured as LOCAL_REDIS_CONN environment variable.</param>
    /// <param name="frontendEndpoint">Optional frontend endpoint reference to be configured for CDN settings.</param>
    /// <param name="scriptName">The npm script to execute. Defaults to "start".</param>
    /// <param name="envFileName">The environment file name to load. Defaults to ".env".</param>
    /// <returns>A resource builder for the Node.js application resource.</returns>
    public static IResourceBuilder<NodeAppResource> AddNodePlatformApp(
        this IDistributedApplicationBuilder builder,
        string name,
        string nodeRootDir,
        string appName,
        int port,
        ReferenceExpression? redisConnectionString = null,
        EndpointReference? frontendEndpoint = null,
        string scriptName = "start",
        string envFileName = ".env")
    {
        var npmApp = builder.AddNpmApp(name, $"{nodeRootDir}/{appName}", scriptName)
            .WithHttpsEndpoint(port: port, targetPort: port, env: "PORT", isProxied: false)
            .WithEnvFile($"{nodeRootDir}/{envFileName}")  // loads .env defaults first
            .WithEnvironment("NODE_TLS_REJECT_UNAUTHORIZED", "0");

        if (frontendEndpoint is not null) {
            npmApp.WithEnvironment("Assets__BaseAddress", frontendEndpoint);
            npmApp.WithEnvironment("CDN_BASE_ADDRESS", frontendEndpoint);
            npmApp.WithEnvironment("CDN_HOST_NAME", frontendEndpoint);
        }

        if (redisConnectionString is not null) {
            npmApp.WithEnvironment("LOCAL_REDIS_CONN", redisConnectionString);
        }

        return npmApp
            .WithReadyLogCheck();
    }

    /// <summary>
    /// Configures the resource with environment variables loaded from a .env file.
    /// </summary>
    /// <typeparam name="T">The resource type that supports environment variables.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="envFilePath">The path to the .env file.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithEnvFile<T>(
        this IResourceBuilder<T> builder,
        string envFilePath)
        where T : IResourceWithEnvironment
    {
        var fullPath = Path.GetFullPath(envFilePath);
        var envVars = File.ReadAllLines(fullPath)
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
            .Select(l => l.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim());

        return builder.WithEnvironment(ctx => {
            foreach (var (key, value) in envVars) {
                ctx.EnvironmentVariables[key] = value;
            }
        });
    }

    /// <summary>
    /// Configures the resource to wait for the specified dependency if it is present.
    /// </summary>
    /// <remarks>If <paramref name="dependency"/> is <see langword="null"/>, no wait dependency is added and
    /// the builder is returned unchanged.</remarks>
    /// <typeparam name="T">The resource type that supports waiting.</typeparam>
    /// <typeparam name="TDep">The dependency resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="dependency">The optional dependency resource builder to wait for.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WaitForIfPresent<T, TDep>(
        this IResourceBuilder<T> builder,
        IResourceBuilder<TDep>? dependency)
        where T : IResource, IResourceWithWaitSupport
        where TDep : IResource
    {
        return dependency is not null ? builder.WaitFor((IResourceBuilder<IResource>)(object)dependency) : builder;
    }

    /// <summary>
    /// Adds a health check that monitors the Node application logs for a specified pattern to determine readiness.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="pattern">The log pattern indicating the application is ready.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<NodeAppResource> WithReadyLogCheck(
        this IResourceBuilder<NodeAppResource> builder,
        string pattern = "Dev server listening")
    {
        var healthCheckKey = $"{builder.Resource.Name}-log-ready";
        var resource = builder.Resource;
        NodeLogHealthCheck? instance = null;

        builder.ApplicationBuilder.Services
            .AddHealthChecks()
            .Add(new HealthCheckRegistration(
                healthCheckKey,
                sp => instance ??= new NodeLogHealthCheck(
                    sp.GetRequiredService<ResourceLoggerService>(),
                    resource,
                    pattern),
                failureStatus: null,
                tags: null));

        return builder.WithHealthCheck(healthCheckKey);
    }
}
