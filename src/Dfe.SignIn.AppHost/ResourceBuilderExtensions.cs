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
}
