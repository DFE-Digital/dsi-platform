using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dotnet;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost;

/// <summary>
/// Provides extension methods for configuring environment variables on resources.
/// </summary>
public static class ProjectResourceBuilderExtensions
{
    /// <summary>
    /// Adds a .NET platform application with the HTTP launch profile, tag and default environment.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> AddDotnetPlatformApp(
        this IDistributedApplicationBuilder builder,
        string name,
        string projectPath)
    {
#pragma warning disable ASPIRERESOURCETRAITS001 // Dashboard tags are experimental in Aspire 17.
        return builder.AddDotnetProject(name, projectPath, options => options.LaunchProfileName = "http")
            .WithTags(ResourceTags.Dotnet)
            .WithDefaultConfiguration(builder.Configuration);
#pragma warning restore ASPIRERESOURCETRAITS001
    }

    /// <summary>
    /// Configures the selected Entity Framework databases.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithEfSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration,
        params string[] databaseNames)
    {
        var ef = configuration.GetSection("EntityFramework");
        foreach (var databaseName in databaseNames) {
            var database = ef.GetSection(databaseName);
            builder
                .WithEnvironment($"EntityFramework__{databaseName}__Host", database["Host"])
                .WithEnvironment($"EntityFramework__{databaseName}__Name", database["Name"])
                .WithEnvironment($"EntityFramework__{databaseName}__Username", database["Username"])
                .WithEnvironment($"EntityFramework__{databaseName}__Password", database["Password"]);
        }

        return builder;
    }

    /// <summary>
    /// Configures the selected Redis connection strings without changing cache database settings.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithRedisConnectionStrings(
        this IResourceBuilder<DotnetProjectResource> builder,
        ReferenceExpression connectionString,
        params string[] sectionNames)
    {
        foreach (var sectionName in sectionNames) {
            builder.WithEnvironment($"{sectionName}__ConnectionString", connectionString);
        }

        return builder;
    }

    /// <summary>
    /// Configures GOV.UK Notify.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithGovNotifySettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        return builder.WithEnvironment("GovNotify__ApiKey", configuration["GovNotify:ApiKey"]);
    }

    /// <summary>
    /// Configures support ticket email delivery.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithSupportEmailSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var supportEmail = configuration.GetSection("RaiseSupportTicketByEmail");

        return builder
            .WithEnvironment("RaiseSupportTicketByEmail__SupportEmailAddress", supportEmail["SupportEmailAddress"])
            .WithEnvironment("RaiseSupportTicketByEmail__EmailTemplateId", supportEmail["EmailTemplateId"]);
    }

    /// <summary>
    /// Configures public API secret encryption.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithPublicApiSecretEncryptionSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        return builder.WithEnvironment("PublicApiSecretEncryption__Key", configuration["PublicApiSecretEncryption:Key"]);
    }

    /// <summary>
    /// Configures External ID client credentials without interactive sign-in settings.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithExternalIdClientSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var externalId = configuration.GetSection("ExternalId");

        return builder
            .WithEnvironment("ExternalId__ClientId", externalId["ClientId"])
            .WithEnvironment("ExternalId__ClientSecret", externalId["ClientSecret"])
            .WithEnvironment("ExternalId__TenantId", externalId["TenantId"]);
    }

    /// <summary>
    /// Configures External ID client credentials and interactive sign-in settings.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithExternalIdSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var externalId = configuration.GetSection("ExternalId");

        return builder
            .WithExternalIdClientSettings(configuration)
            .WithEnvironment("ExternalId__Authority", externalId["Authority"])
            .WithEnvironment("ExternalId__Instance", externalId["Instance"]);
    }

    /// <summary>
    /// Configures the OpenID Connect client.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithOidcSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var oidc = configuration.GetSection("Oidc");

        return builder
            .WithEnvironment("Oidc__ClientId", oidc["ClientId"])
            .WithEnvironment("Oidc__ClientSecret", oidc["ClientSecret"])
            .WithEnvironment("Oidc__Authority", oidc["Authority"])
            .WithEnvironment("Oidc__MetadataAddress", oidc["MetadataAddress"]);
    }

    /// <summary>
    /// Configures session duration and expiry notifications.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithSessionSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var session = configuration.GetSection("Session");

        return builder
            .WithEnvironment("Session__DurationInMinutes", session["DurationInMinutes"])
            .WithEnvironment("Session__NotifyRemainingMinutes", session["NotifyRemainingMinutes"]);
    }

    /// <summary>
    /// Configures bearer token validation.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithBearerTokenSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        return builder.WithEnvironment("BearerToken__ValidAudience", configuration["BearerToken:ValidAudience"]);
    }

    /// <summary>
    /// Configures the organisation selection address.
    /// </summary>
    public static IResourceBuilder<DotnetProjectResource> WithSelectOrganisationSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
        IConfiguration configuration)
    {
        var selectOrg = configuration.GetSection("SelectOrganisation");

        return builder.WithEnvironment("SelectOrganisation__SelectOrganisationBaseAddress", selectOrg["SelectOrganisationBaseAddress"]);
    }

    /// <summary>
    /// Configures the resource with platform-related environment variables from the application configuration.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<DotnetProjectResource> WithPlatformSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
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
    public static IResourceBuilder<DotnetProjectResource> WithServiceBusAuditingSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
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
    public static IResourceBuilder<DotnetProjectResource> WithLocalInternalApi(
        this IResourceBuilder<DotnetProjectResource> builder,
        IResourceBuilder<DotnetProjectResource> internalApi)
    {
        return builder
            .WithEnvironment("InternalApiClient__BaseAddress", internalApi.GetEndpoint("https"))
            .WaitFor(internalApi);
    }

    /// <summary>
    /// Configures the resource with notification-related environment variables from the application configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IResourceBuilder<DotnetProjectResource> WithDefaultConfiguration(
        this IResourceBuilder<DotnetProjectResource> builder,
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
    public static IResourceBuilder<DotnetProjectResource> WithInternalApiSettings(
        this IResourceBuilder<DotnetProjectResource> builder,
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
