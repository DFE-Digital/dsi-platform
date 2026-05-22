using Microsoft.Extensions.Configuration;

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

    public static IResourceBuilder<NodeAppResource> AddNodePlatformApp(
        this IDistributedApplicationBuilder builder,
        string name,
        string nodeRootDir,
        string appName,
        int port,
        ReferenceExpression? redisConnectionString = null,
        string scriptName = "start",
        string envFileName = ".env")
    {
        var npmApp = builder.AddNpmApp(name, $"{nodeRootDir}/{appName}", scriptName)
            .WithHttpsEndpoint(port: port, targetPort: port, env: "PORT", isProxied: false)
            .WithEnvFile($"{nodeRootDir}/{envFileName}")  // loads .env defaults first
            .WithEnvironment("NODE_TLS_REJECT_UNAUTHORIZED", "0");

        if (redisConnectionString is not null) {
            npmApp.WithEnvironment("LOCAL_REDIS_CONN", redisConnectionString);
        }

        return npmApp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="envFilePath"></param>
    /// <returns></returns>
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
}
