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
    /// Forwards all non-null values from a configuration section as environment variables,
    /// converting configuration path separators (<c>:</c>) to double underscores.
    /// </summary>
    /// <typeparam name="T">The resource type that supports environment variables.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section to forward (e.g. <c>EntityFramework</c>).</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithConfigurationSection<T>(
        this IResourceBuilder<T> builder,
        IConfiguration configuration,
        string sectionName)
        where T : IResourceWithEnvironment
    {
        foreach (var (key, value) in EnumerateSectionAsEnvironmentVariables(configuration, sectionName)) {
            builder.WithEnvironment(key, value);
        }

        return builder;
    }

    /// <summary>
    /// Converts a configuration section into environment variable key/value pairs.
    /// </summary>
    internal static IEnumerable<KeyValuePair<string, string>> EnumerateSectionAsEnvironmentVariables(
        IConfiguration configuration,
        string sectionName)
    {
        foreach (var (key, value) in configuration.GetSection(sectionName).AsEnumerable(makePathsRelative: false)) {
            if (value is null) {
                continue;
            }

            yield return new KeyValuePair<string, string>(key.Replace(":", "__"), value);
        }
    }

    /// <summary>
    /// Sets <c>ASPNETCORE_ENVIRONMENT</c> and local-auth bypass from configuration.
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithDsiEnvironment(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Local";

        return builder
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
            .WithEnvironment("Authentication__LocalBypass", environment == "Local" ? "true" : "false");
    }

    /// <summary>
    /// Sets Redis connection strings for one or more named cache configuration sections.
    /// </summary>
    /// <typeparam name="T">The resource type that supports environment variables.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="connectionString">The Redis connection string expression.</param>
    /// <param name="cacheNames">Cache section names (e.g. <c>GeneralRedisCache</c>).</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithRedisCaches<T>(
        this IResourceBuilder<T> builder,
        ReferenceExpression connectionString,
        params string[] cacheNames)
        where T : IResourceWithEnvironment
    {
        foreach (var cacheName in cacheNames) {
            builder.WithEnvironment($"{cacheName}__ConnectionString", connectionString);
        }

        return builder;
    }

    /// <summary>
    /// Points the resource at the Internal API HTTPS endpoint and waits for it to be ready.
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="internalApi">The Internal API resource.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithInternalApi(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ProjectResource> internalApi)
    {
        return builder
            .WithEnvironment("InternalApiClient__BaseAddress", internalApi.GetEndpoint("https"))
            .WaitFor(internalApi);
    }

    /// <summary>
    /// Sets asset base address from the frontend endpoint and optionally forwards asset version config.
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="frontendEndpoint">The frontend HTTP endpoint.</param>
    /// <param name="configuration">Optional configuration used to forward <c>Assets:FrontendVersion</c>.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithFrontendAssets(
        this IResourceBuilder<ProjectResource> builder,
        EndpointReference frontendEndpoint,
        IConfiguration? configuration = null)
    {
        builder.WithEnvironment("Assets__BaseAddress", frontendEndpoint);

        if (configuration is not null) {
            builder.WithEnvironment(
                "Assets__FrontendVersion",
                configuration["Assets:FrontendVersion"] ?? string.Empty);
        }

        return builder;
    }

    /// <summary>
    /// Adds a dashboard command that runs a PowerShell stub to generate an API key.
    /// </summary>
    /// <remarks>
    /// Invokes <c>scripts/Generate-ApiKey.ps1</c>. Stdout is shown immediately in the Aspire
    /// dashboard result dialog. Replace the script body with real generation logic when ready.
    /// </remarks>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="displayName">Optional dashboard label for the command.</param>
    /// <param name="apiKind">Passed to the script as <c>-ApiKind</c> (<c>internal</c> or <c>public</c>).</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithGenerateApiKeyCommand(
        this IResourceBuilder<ProjectResource> builder,
        string displayName = "Generate API key",
        string apiKind = "public")
    {
        var scriptPath = Path.Combine(
            builder.ApplicationBuilder.AppHostDirectory,
            "scripts",
            "Generate-ApiKey.ps1");

#pragma warning disable ASPIREPROCESSCOMMAND001 // WithProcessCommand is experimental.
#pragma warning disable ASPIREINTERACTION001 // InteractionInput / CommandOptions.Arguments are experimental.
        return builder.WithProcessCommand(
            commandName: "generate-api-key",
            displayName: displayName,
            processSpecFactory: context => {
                var clientName = context.Arguments.GetString("clientName") ?? "local-client";
                var description = context.Arguments.GetString("description") ?? string.Empty;
                var environment = context.Arguments.GetString("environment") ?? "Local";
                var expiresDays = context.Arguments.GetString("expiresDays") ?? "90";
                var createInactive = context.Arguments.GetString("createInactive") ?? "false";

                return new ProcessCommandSpec("pwsh")
                {
                    WorkingDirectory = builder.ApplicationBuilder.AppHostDirectory,
                    Arguments =
                    [
                        "-NoProfile",
                        "-File",
                        scriptPath,
                        "-ClientName",
                        clientName,
                        "-ApiKind",
                        apiKind,
                        "-Description",
                        description,
                        "-Environment",
                        environment,
                        "-ExpiresDays",
                        expiresDays,
                        "-CreateInactive",
                        createInactive,
                    ],
                };
            },
            commandOptions: new ProcessCommandOptions
            {
                Description = "Generate a local development API key via PowerShell (stub).",
                IconName = "Key",
                IconVariant = IconVariant.Filled,
                DisplayImmediately = true,
                Arguments =
                [
                    new InteractionInput
                    {
                        Name = "clientName",
                        Label = "Client name",
                        InputType = InputType.Text,
                        Required = true,
                        Value = "local-client",
                        Placeholder = "e.g. my-integration",
                    },
                    new InteractionInput
                    {
                        Name = "description",
                        Label = "Description",
                        InputType = InputType.Text,
                        Required = false,
                        Placeholder = "Optional note for this key",
                    },
                    new InteractionInput
                    {
                        Name = "environment",
                        Label = "Environment",
                        InputType = InputType.Choice,
                        Required = true,
                        Value = "Local",
                        Options =
                        [
                            KeyValuePair.Create("Local", "Local"),
                            KeyValuePair.Create("Development", "Development"),
                            KeyValuePair.Create("Test", "Test"),
                        ],
                    },
                    new InteractionInput
                    {
                        Name = "expiresDays",
                        Label = "Expires in (days)",
                        InputType = InputType.Number,
                        Required = true,
                        Value = "90",
                    },
                    new InteractionInput
                    {
                        Name = "createInactive",
                        Label = "Create as inactive",
                        InputType = InputType.Boolean,
                        Required = false,
                        Value = "false",
                    },
                ],
            });
#pragma warning restore ASPIREINTERACTION001
#pragma warning restore ASPIREPROCESSCOMMAND001
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
            .WithEnvFile($"{nodeRootDir}/{envFileName}")
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
