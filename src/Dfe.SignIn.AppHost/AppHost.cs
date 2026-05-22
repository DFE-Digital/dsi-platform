using System.Reflection;
using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);

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
var redisConnectionString = ReferenceExpression.Create(
    $"redis://{redisTcpEndpoint.Property(EndpointProperty.Host)}:{redisTcpEndpoint.Property(EndpointProperty.Port)}");

var frontend = builder.AddDockerfile("infra-frontend", "../../", "docker/frontend/Dockerfile")
    .WithHttpEndpoint(targetPort: 8080, name: "http");

// Extract shared configuration sections to easily reuse them across projects
var govNotifyConfig = builder.Configuration.GetSection("GovNotify");
var supportEmailConfig = builder.Configuration.GetSection("RaiseSupportTicketByEmail");
var oidcConfig = builder.Configuration.GetSection("Oidc");
var externalIdConfig = builder.Configuration.GetSection("ExternalId");
var sessionConfig = builder.Configuration.GetSection("Session");
var bearerTokenConfig = builder.Configuration.GetSection("BearerToken");
var publicApiSecretConfig = builder.Configuration.GetSection("PublicApiSecretEncryption");
var selectOrgConfig = builder.Configuration.GetSection("SelectOrganisation");
var internalApiConfig = builder.Configuration.GetSection("InternalApiClient");
var efConfig = builder.Configuration.GetSection("EntityFramework");

var dotNetComponents = builder.Configuration.GetSection("Components:DotNet");
var nodeComponents = builder.Configuration.GetSection("Components:Node");

if (dotNetComponents.GetValue("HelpEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_Web_Help>("app-help", launchProfileName: "http")
    .WithSharedConfiguration(builder.Configuration, frontend.GetEndpoint("http"))
    .WithEnvironment("InteractionsRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("GovNotify__ApiKey", govNotifyConfig["ApiKey"])
    .WithEnvironment("RaiseSupportTicketByEmail__SupportEmailAddress", supportEmailConfig["SupportEmailAddress"])
    .WithEnvironment("RaiseSupportTicketByEmail__EmailTemplateId", supportEmailConfig["EmailTemplateId"])
    .WaitFor(frontend)
    .WaitFor(redis);
}

if (dotNetComponents.GetValue("ProfileEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_Web_Profile>("app-profile", launchProfileName: "http")
    .WithSharedConfiguration(builder.Configuration, frontend.GetEndpoint("http"))
    .WithEnvironment("GeneralRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("SessionRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("TokenRedisCache__ConnectionString", redisConnectionString)
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
    .WaitFor(frontend)
    .WaitFor(redis);
}

if (dotNetComponents.GetValue("PublicApiEnabled", true)) {
    builder.AddProject<Projects.Dfe_SignIn_PublicApi>("app-public-api", launchProfileName: "http")
    .WithSharedConfiguration(builder.Configuration, frontend.GetEndpoint("http"))
    .WithEnvironment("SelectOrganisationSessionRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("InteractionsRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("BearerToken__ValidAudience", bearerTokenConfig["ValidAudience"])
    .WithEnvironment("PublicApiSecretEncryption__Key", publicApiSecretConfig["Key"])
    .WithEnvironment("SelectOrganisation__SelectOrganisationBaseAddress", selectOrgConfig["SelectOrganisationBaseAddress"])
    .WithEnvironment("InternalApiClient__Access__BaseAddress", internalApiConfig["Access:BaseAddress"])
    .WithEnvironment("InternalApiClient__Organisations__BaseAddress", internalApiConfig["Organisations:BaseAddress"])
    .WaitFor(redis);
}

var internalApi = builder.AddProject<Projects.Dfe_SignIn_InternalApi>("app-internal-api", launchProfileName: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Local")
    .WithEnvironment("EntityFramework__Directories__Host", efConfig["Directories:Host"])
    .WithEnvironment("EntityFramework__Directories__Name", efConfig["Directories:Name"])
    .WithEnvironment("EntityFramework__Directories__Username", efConfig["Directories:Username"])
    .WithEnvironment("EntityFramework__Directories__Password", efConfig["Directories:Password"])
    .WithEnvironment("EntityFramework__Organisations__Host", efConfig["Organisations:Host"])
    .WithEnvironment("EntityFramework__Organisations__Name", efConfig["Organisations:Name"])
    .WithEnvironment("EntityFramework__Organisations__Username", efConfig["Organisations:Username"])
    .WithEnvironment("EntityFramework__Organisations__Password", efConfig["Organisations:Password"])
    .WithEnvironment("InternalApiClient__ClientId", internalApiConfig["ClientId"])
    .WithEnvironment("InternalApiClient__ClientSecret", internalApiConfig["ClientSecret"])
    .WithEnvironment("InternalApiClient__Tenant", internalApiConfig["Tenant"])
    .WithEnvironment("InternalApiClient__HostUrl", internalApiConfig["HostUrl"])
    .WithEnvironment("InternalApiClient__Search__BaseAddress", internalApiConfig["Search:BaseAddress"])
    .WithEnvironment("InternalApiClient__Access__BaseAddress", internalApiConfig["Access:BaseAddress"])
    .WithEnvironment("InternalApiClient__Organisations__BaseAddress", internalApiConfig["Organisations:BaseAddress"])
    .WithEnvironment("InternalApiClient__Directories__BaseAddress", internalApiConfig["Directories:BaseAddress"])
    .WithEnvironment("InternalApiClient__Applications__BaseAddress", internalApiConfig["Applications:BaseAddress"])
    .WithEnvironment("InternalApiClient__UseProxy", "false");

var nodeRootDir = builder.Configuration["NodePlatformDirectory"]
    ?? throw new InvalidOperationException("NodePlatformDirectory is not configured.");

var nodeEnvFileName = builder.Configuration["NodeEnvFileName"]
    ?? throw new InvalidOperationException("NodeEnvFileName is not configured.");

// Node components
IResourceBuilder<NodeAppResource>? oidc = null;
if (nodeComponents.GetValue("OidcEnabled", true)) {
    oidc = builder.AddNodePlatformApp("node-oidc", nodeRootDir, "login.dfe.oidc", 4436, redisConnectionString, envFileName: nodeEnvFileName, scriptName: "dev")
    .WithNpmPackageInstallation()
    .WaitFor(redis);
}

IResourceBuilder<NodeAppResource>? interactor = null;
if (nodeComponents.GetValue("InteractionsEnabled", true)) {
    interactor = builder.AddNodePlatformApp("node-interactions", nodeRootDir, "login.dfe.interactions", 4431, redisConnectionString, envFileName: nodeEnvFileName, scriptName: "dev")
    .WithNpmPackageInstallation();

    if (oidc != null) {
        interactor.WaitFor(oidc);
    }
}

if (nodeComponents.GetValue("ServicesEnabled", true)) {
    var services = builder.AddNodePlatformApp("node-services", nodeRootDir, "login.dfe.services", 41012, redisConnectionString, envFileName: nodeEnvFileName)
    .WithNpmPackageInstallation();

    if (oidc != null) {
        services.WaitFor(oidc);
    }

    if (interactor != null) {
        services.WaitFor(interactor);
    }
}

builder.AddExecutable("tool-tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy");

builder.Build().Run();
