using System.Reflection;
using Azure.Identity;
using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);

var appConfigurationConnectionString = builder.Configuration.GetConnectionString("AppConfiguration");
if (!string.IsNullOrEmpty(appConfigurationConnectionString)) {
    var appConfigurationTag = builder.Configuration["AppConfiguration:Tag"];
    if (string.IsNullOrEmpty(appConfigurationTag)) {
        throw new InvalidOperationException("AppConfiguration Tag missing from configuration.");
    }

    builder.Configuration.AddAzureAppConfiguration(options => {
        options.Connect(appConfigurationConnectionString)
               .Select(KeyFilter.Any, appConfigurationTag)
               .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
    });

    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);
}

#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var redis = builder.AddRedis("infra-redis")
    .WithPassword(null)
    .WithEndpointProxySupport(false)
    .WithImage("redis", "latest")
    .WithDataVolume()
    .WithoutHttpsCertificate()
    .WithRedisInsight();
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var redisTcpEndpoint = redis.GetEndpoint("tcp");

// .NET Format (No prefix, StackExchange.Redis expects "host:port")
var dotnetRedisConnectionString = ReferenceExpression.Create(
    $"{redisTcpEndpoint.Property(EndpointProperty.Host)}:{redisTcpEndpoint.Property(EndpointProperty.Port)}");

// Node.js Format (Requires "redis://" URI prefix)
var nodeRedisConnectionString = ReferenceExpression.Create(
    $"redis://{redisTcpEndpoint.Property(EndpointProperty.Host)}:{redisTcpEndpoint.Property(EndpointProperty.Port)}");

var frontend = builder.AddDockerfile("infra-frontend", "../../", "docker/frontend/Dockerfile")
    .WithHttpEndpoint(targetPort: 8080, name: "http");

var frontendEndpoint = frontend.GetEndpoint("http");

// Extract shared configuration sections to easily reuse them across projects
var govNotifyConfig = builder.Configuration.GetSection("GovNotify");
var supportEmailConfig = builder.Configuration.GetSection("RaiseSupportTicketByEmail");
var oidcConfig = builder.Configuration.GetSection("Oidc");
var externalIdConfig = builder.Configuration.GetSection("ExternalId");
var sessionConfig = builder.Configuration.GetSection("Session");
var bearerTokenConfig = builder.Configuration.GetSection("BearerToken");
var publicApiSecretConfig = builder.Configuration.GetSection("PublicApiSecretEncryption");
var selectOrgConfig = builder.Configuration.GetSection("SelectOrganisation");
var efConfig = builder.Configuration.GetSection("EntityFramework");
var generalRedisConfig = builder.Configuration.GetSection("GeneralRedisCache");
var serviceBusConfig = builder.Configuration.GetSection("ServiceBus");

var dotNetComponents = builder.Configuration.GetSection("Components:DotNet");
var nodeComponents = builder.Configuration.GetSection("Components:Node");

if (dotNetComponents.GetValue("HelpEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_Web_Help>("app-help", launchProfileName: "http")
        .WithDefaultConfiguration(builder.Configuration)
        .WithHostedInternalApiSettings(builder.Configuration)
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithEnvironment("InteractionsRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("GovNotify__ApiKey", govNotifyConfig["ApiKey"])
        .WithEnvironment("RaiseSupportTicketByEmail__SupportEmailAddress", supportEmailConfig["SupportEmailAddress"])
        .WithEnvironment("RaiseSupportTicketByEmail__EmailTemplateId", supportEmailConfig["EmailTemplateId"])
        .WaitFor(redis);
}

var internalApi = builder.AddProject<Projects.Dfe_SignIn_InternalApi>("app-internal-api", launchProfileName: "http")
    .WithDefaultConfiguration(builder.Configuration)
    .WithServiceBusAuditingSettings(serviceBusConfig)
    .WithHostedInternalApiSettings(builder.Configuration)
    .WithEnvironment("Authentication__LocalBypass", builder.Configuration["ASPNETCORE_ENVIRONMENT"] == "Local" ? "true" : "false")
    .WithEnvironment("EntityFramework__Directories__Host", efConfig["Directories:Host"])
    .WithEnvironment("EntityFramework__Directories__Name", efConfig["Directories:Name"])
    .WithEnvironment("EntityFramework__Directories__Username", efConfig["Directories:Username"])
    .WithEnvironment("EntityFramework__Directories__Password", efConfig["Directories:Password"])
    .WithEnvironment("EntityFramework__Organisations__Host", efConfig["Organisations:Host"])
    .WithEnvironment("EntityFramework__Organisations__Name", efConfig["Organisations:Name"])
    .WithEnvironment("EntityFramework__Organisations__Username", efConfig["Organisations:Username"])
    .WithEnvironment("EntityFramework__Organisations__Password", efConfig["Organisations:Password"])
    .WithEnvironment("PublicApiSecretEncryption__Key", publicApiSecretConfig["Key"])
    .WithEnvironment("ExternalId__ClientId", externalIdConfig["ClientId"])
    .WithEnvironment("ExternalId__ClientSecret", externalIdConfig["ClientSecret"])
    .WithEnvironment("ExternalId__TenantId", externalIdConfig["TenantId"])
    .WithEnvironment("GeneralRedisCache__ConnectionString", dotnetRedisConnectionString)
    .WithEnvironment("GeneralRedisCache__DatabaseNumber", generalRedisConfig["DatabaseNumber"])
    .WithEnvironment("BullMQ__ConnectionString", dotnetRedisConnectionString)
    .WithEnvironment("GovNotify__ApiKey", govNotifyConfig["ApiKey"]);

if (dotNetComponents.GetValue("ProfileEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_Web_Profile>("app-profile", launchProfileName: "http")
        .WithDefaultConfiguration(builder.Configuration)
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithServiceBusAuditingSettings(serviceBusConfig)
        .WithHostedInternalApiSettings(builder.Configuration) //todo: is this still needed?
        .WithLocalInternalApi(internalApi)
        .WithEnvironment("GeneralRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("SessionRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("TokenRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("Oidc__ClientId", oidcConfig["ClientId"])
        .WithEnvironment("Oidc__ClientSecret", oidcConfig["ClientSecret"])
        .WithEnvironment("Oidc__Authority", oidcConfig["Authority"])
        .WithEnvironment("Oidc__MetadataAddress", oidcConfig["MetadataAddress"])
        .WithEnvironment("ExternalId__ClientId", externalIdConfig["ClientId"])
        .WithEnvironment("ExternalId__ClientSecret", externalIdConfig["ClientSecret"])
        .WithEnvironment("ExternalId__Authority", externalIdConfig["Authority"])
        .WithEnvironment("ExternalId__Instance", externalIdConfig["Instance"])
        .WithEnvironment("ExternalId__TenantId", externalIdConfig["TenantId"])
        .WithEnvironment("Session__DurationInMinutes", sessionConfig["DurationInMinutes"])
        .WithEnvironment("Session__NotifyRemainingMinutes", sessionConfig["NotifyRemainingMinutes"])
        .WaitFor(redis);
}

if (dotNetComponents.GetValue("PublicApiEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_PublicApi>("app-public-api", launchProfileName: "http")
        .WithDefaultConfiguration(builder.Configuration)
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithServiceBusAuditingSettings(serviceBusConfig)
        .WithHostedInternalApiSettings(builder.Configuration) //todo: is this still needed?
        .WithLocalInternalApi(internalApi)
        .WithEnvironment("SelectOrganisationSessionRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("InteractionsRedisCache__ConnectionString", dotnetRedisConnectionString)
        .WithEnvironment("BearerToken__ValidAudience", bearerTokenConfig["ValidAudience"])
        .WithEnvironment("PublicApiSecretEncryption__Key", publicApiSecretConfig["Key"])
        .WithEnvironment("SelectOrganisation__SelectOrganisationBaseAddress", selectOrgConfig["SelectOrganisationBaseAddress"])
        .WithEnvironment("EntityFramework__Organisations__Username", efConfig["Organisations:Username"])
        .WithEnvironment("EntityFramework__Organisations__Password", efConfig["Organisations:Password"])
        .WithEnvironment("EntityFramework__Organisations__Name", efConfig["Organisations:Name"])
        .WithEnvironment("EntityFramework__Organisations__Host", efConfig["Organisations:Host"])
        .WithEnvironment("EntityFramework__Directories__Username", efConfig["Directories:Username"])
        .WithEnvironment("EntityFramework__Directories__Password", efConfig["Directories:Password"])
        .WithEnvironment("EntityFramework__Directories__Name", efConfig["Directories:Name"])
        .WithEnvironment("EntityFramework__Directories__Host", efConfig["Directories:Host"])
        .WithEnvironment("EntityFramework__Audit__Username", efConfig["Audit:Username"])
        .WithEnvironment("EntityFramework__Audit__Password", efConfig["Audit:Password"])
        .WithEnvironment("EntityFramework__Audit__Name", efConfig["Audit:Name"])
        .WithEnvironment("EntityFramework__Audit__Host", efConfig["Audit:Host"])
        .WaitFor(internalApi)
        .WaitFor(redis);
}

var nodeRootDir = builder.Configuration["NodePlatformDirectory"]
    ?? throw new InvalidOperationException("NodePlatformDirectory is not configured.");

var nodeEnvFileName = builder.Configuration["NodeEnvFileName"]
    ?? throw new InvalidOperationException("NodeEnvFileName is not configured.");

// Node components
IResourceBuilder<NodeAppResource>? oidc = null;
if (nodeComponents.GetValue("OidcEnabled", true)) {
    oidc = builder.AddNodePlatformApp("node-oidc", nodeRootDir, "login.dfe.oidc", 4436, nodeRedisConnectionString, frontendEndpoint, envFileName: nodeEnvFileName, scriptName: "dev")
    .WithNpmPackageInstallation()
    .WaitFor(redis);
}

IResourceBuilder<NodeAppResource>? interactor = null;
if (nodeComponents.GetValue("InteractionsEnabled", true)) {
    interactor = builder.AddNodePlatformApp("node-interactions", nodeRootDir, "login.dfe.interactions", 4431, nodeRedisConnectionString, frontendEndpoint, envFileName: nodeEnvFileName, scriptName: "dev")
    .WithNpmPackageInstallation()
    .WaitForIfPresent(oidc);
}

if (nodeComponents.GetValue("ServicesEnabled", true)) {
    builder.AddNodePlatformApp("node-services", nodeRootDir, "login.dfe.services", 41012, nodeRedisConnectionString, frontendEndpoint, envFileName: nodeEnvFileName)
    .WithNpmPackageInstallation()
    .WaitForIfPresent(oidc)
    .WaitForIfPresent(interactor);
}

if (nodeComponents.GetValue("Run-TlsProxy", false)) {
    builder.AddExecutable("tool-tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy");
}
await builder.Build().RunAsync();
