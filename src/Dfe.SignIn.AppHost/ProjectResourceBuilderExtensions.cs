using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost;

/// <summary>
/// Provides extension methods for configuring environment variables on resources.
/// </summary>
public static class ProjectResourceBuilderExtensions
{
    /// <summary>
    /// Configures the resource with platform-related environment variables from the application configuration.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithPlatformSettings(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration)
    {
        var platform = configuration.GetSection("Platform");

        return builder
            .WithEnvironment("Platform__SurveyUrl", platform["SurveyUrl"])
            .WithEnvironment("Platform__HelpUrl", platform["HelpUrl"])
            .WithEnvironment("Platform__ManageUrl", platform["ManageUrl"])
            .WithEnvironment("Platform__ProfileUrl", platform["ProfileUrl"])
            .WithEnvironment("Platform__ServicesUrl", platform["ServicesUrl"])
            .WithEnvironment("Platform__SupportUrl", platform["SupportUrl"])
            .WithEnvironment("Platform__CookiesUrl", platform["CookiesUrl"])
            .WithEnvironment("Platform__TermsUrl", platform["TermsUrl"])
            .WithEnvironment("Platform__PrivacyUrl", platform["PrivacyUrl"])
            .WithEnvironment("Platform__AccessibilityUrl", platform["AccessibilityUrl"])
            .WithEnvironment("Platform__ContactUrl", platform["ContactUrl"]);
    }

    /// <summary>
    /// Configures the resource with Service Bus auditing-related environment variables from the application configuration.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithServiceBusAuditingSettings(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration)
    {
        var serviceBusConfig = configuration.GetSection("ServiceBus");
        return builder
            .WithEnvironment("ServiceBus__AuditTopic__TopicName", serviceBusConfig["AuditTopic:TopicName"])
            .WithEnvironment("ServiceBus__AuditTopic__SubscriptionName", serviceBusConfig["AuditTopic:SubscriptionName"])
            .WithEnvironment("ServiceBus__Namespace", serviceBusConfig["Namespace"]);
    }

    /// <summary>
    /// Configures the resource with the internal API client base address and sets up a dependency on the internal API resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="internalApi">The internal API resource builder.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithLocalInternalApi(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ProjectResource> internalApi)
    {
        return builder
            .WithEnvironment("InternalApiClient__BaseAddress", internalApi.GetEndpoint("https"))
            .WaitFor(internalApi);
    }

    /// <summary>
    /// Configures the resource with frontend asset settings, including the internal API client base address and a dependency on the internal API resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="frontendAssets"></param>
    /// <param name="configuration"></param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithFrontendAssets(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ContainerResource> frontendAssets,
        IConfiguration configuration)
    {
        var assets = configuration.GetSection("Assets");
        var appHostConfig = configuration.GetSection("Components:Frontend");
        var useHostedAssets = appHostConfig.GetValue<bool>("UseHostedAssets");

        if (useHostedAssets) {
            builder = builder.WithEnvironment("Assets__BaseAddress", assets["BaseAddress"]);
        }
        else {
            builder = builder.WithEnvironment("Assets__BaseAddress", frontendAssets.GetEndpoint("http"));
        }

        return builder
            .WithEnvironment("Assets__FrontendVersion", assets["FrontendVersion"])
            .WaitFor(frontendAssets);
    }

    /// <summary>
    /// Configures the resource with notification-related environment variables from the application configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IResourceBuilder<ProjectResource> WithDefaultConfiguration(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Local";
        return builder
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment);
    }

    /// <summary>
    /// Configures a project resource with shared configuration settings from the application configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <param name="configuration">The application configuration containing the settings.</param>
    /// <returns>The configured resource builder.</returns>
    public static IResourceBuilder<ProjectResource> WithHostedInternalApiSettings(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration)
    {
        var internalApiConfig = configuration.GetSection("InternalApiClient");

        builder
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
            .WithEnvironment("InternalApiClient__Access__BaseAddress", internalApiConfig["Access:BaseAddress"])
            .WithEnvironment("InternalApiClient__Organisations__BaseAddress", internalApiConfig["Organisations:BaseAddress"]);

        return builder;
    }
}
