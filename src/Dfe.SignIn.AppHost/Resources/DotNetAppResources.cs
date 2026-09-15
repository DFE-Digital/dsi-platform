using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.Resources;

/// <summary>
/// Registers .NET Aspire project resources (APIs and web apps).
/// </summary>
public static class DotNetAppResources
{
    /// <summary>
    /// Adds the Internal API project (always started).
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddInternalApi(
        this IDistributedApplicationBuilder builder,
        PlatformInfrastructure infra)
    {
        var configuration = builder.Configuration;

        return builder.AddProject<Projects.Dfe_SignIn_InternalApi>("app-internal-api", launchProfileName: "http")
            .WithDsiEnvironment(configuration)
            .WithConfigurationSection(configuration, "EntityFramework")
            .WithConfigurationSection(configuration, "PublicApiSecretEncryption")
            .WithConfigurationSection(configuration, "InternalApiClient")
            .WithConfigurationSection(configuration, "ExternalId")
            .WithConfigurationSection(configuration, "GovNotify")
            .WithConfigurationSection(configuration, "ServiceBus")
            .WithConfigurationSection(configuration, "GeneralRedisCache")
            .WithRedisCaches(infra.DotNetRedisConnectionString, "GeneralRedisCache", "BullMQ")
            .WithEnvironment("InternalApiClient__UseProxy", "false")
            .WithGenerateApiKeyCommand("Generate Internal API key", apiKind: "internal")
            .WaitFor(infra.Redis);
    }

    /// <summary>
    /// Adds the Help web app when enabled.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddHelpApp(
        this IDistributedApplicationBuilder builder,
        PlatformInfrastructure infra,
        IResourceBuilder<ProjectResource> internalApi)
    {
        var configuration = builder.Configuration;

        return builder.AddProject<Projects.Dfe_SignIn_Web_Help>("app-help", launchProfileName: "http")
            .WithDsiEnvironment(configuration)
            .WithConfigurationSection(configuration, "Platform")
            .WithConfigurationSection(configuration, "InternalApiClient")
            .WithConfigurationSection(configuration, "GovNotify")
            .WithConfigurationSection(configuration, "RaiseSupportTicketByEmail")
            .WithFrontendAssets(infra.FrontendEndpoint, configuration)
            .WithInternalApi(internalApi)
            .WithRedisCaches(infra.DotNetRedisConnectionString, "InteractionsRedisCache")
            .WaitFor(infra.Frontend)
            .WaitFor(infra.Redis);
    }

    /// <summary>
    /// Adds the Profile web app when enabled.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddProfileApp(
        this IDistributedApplicationBuilder builder,
        PlatformInfrastructure infra,
        IResourceBuilder<ProjectResource> internalApi)
    {
        var configuration = builder.Configuration;

        return builder.AddProject<Projects.Dfe_SignIn_Web_Profile>("app-profile", launchProfileName: "http")
            .WithDsiEnvironment(configuration)
            .WithConfigurationSection(configuration, "Platform")
            .WithConfigurationSection(configuration, "InternalApiClient")
            .WithConfigurationSection(configuration, "Oidc")
            .WithConfigurationSection(configuration, "ExternalId")
            .WithConfigurationSection(configuration, "Session")
            .WithConfigurationSection(configuration, "ServiceBus")
            .WithFrontendAssets(infra.FrontendEndpoint, configuration)
            .WithInternalApi(internalApi)
            .WithRedisCaches(
                infra.DotNetRedisConnectionString,
                "GeneralRedisCache",
                "SessionRedisCache",
                "TokenRedisCache")
            .WaitFor(infra.Frontend)
            .WaitFor(infra.Redis);
    }

    /// <summary>
    /// Adds the Public API when enabled.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddPublicApi(
        this IDistributedApplicationBuilder builder,
        PlatformInfrastructure infra,
        IResourceBuilder<ProjectResource> internalApi)
    {
        var configuration = builder.Configuration;

        return builder.AddProject<Projects.Dfe_SignIn_PublicApi>("app-public-api", launchProfileName: "http")
            .WithDsiEnvironment(configuration)
            .WithConfigurationSection(configuration, "Platform")
            .WithConfigurationSection(configuration, "InternalApiClient")
            .WithConfigurationSection(configuration, "BearerToken")
            .WithConfigurationSection(configuration, "PublicApiSecretEncryption")
            .WithConfigurationSection(configuration, "SelectOrganisation")
            .WithConfigurationSection(configuration, "EntityFramework")
            .WithConfigurationSection(configuration, "ServiceBus")
            .WithFrontendAssets(infra.FrontendEndpoint)
            .WithInternalApi(internalApi)
            .WithRedisCaches(
                infra.DotNetRedisConnectionString,
                "SelectOrganisationSessionRedisCache",
                "InteractionsRedisCache")
            .WithGenerateApiKeyCommand("Generate Public API key", apiKind: "public")
            .WaitFor(infra.Redis);
    }
}
