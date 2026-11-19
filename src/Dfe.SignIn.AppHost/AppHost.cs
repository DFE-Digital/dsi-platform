using Aspire.Hosting.JavaScript;
using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddAppHostConfiguration();

// Infrastructure
#pragma warning disable ASPIRERESOURCETRAITS001 // Dashboard tags are experimental in Aspire 17.
var redis = builder.AddPlatformRedis("redis");
var dotnetRedisConnectionString = redis.GetDotnetConnectionString();
var nodeRedisConnectionString = redis.GetNodeConnectionString();

var frontend = builder.AddDockerfile("frontend", "../../", "docker/frontend/Dockerfile")
    .WithTags(ResourceTags.Infrastructure)
    .WithHttpEndpoint(targetPort: 8080, name: "http");

// .NET applications
var dotNetComponents = builder.Configuration.GetSection("Components:DotNet");

if (dotNetComponents.GetValue("HelpEnabled", true)) {
    builder.AddDotnetPlatformApp("help", "../Dfe.SignIn.Web.Help/Dfe.SignIn.Web.Help.csproj")
        .WithInternalApiSettings(builder.Configuration)
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithRedisConnectionStrings(dotnetRedisConnectionString, "InteractionsRedisCache")
        .WithGovNotifySettings(builder.Configuration)
        .WithSupportEmailSettings(builder.Configuration)
        .WaitFor(redis);
}

var internalApi = builder.AddDotnetPlatformApp("internal-api", "../Dfe.SignIn.InternalApi/Dfe.SignIn.InternalApi.csproj")
    .WithServiceBusAuditingSettings(builder.Configuration)
    .WithInternalApiSettings(builder.Configuration)
    .WithEnvironment("Authentication__LocalBypass", builder.Configuration["ASPNETCORE_ENVIRONMENT"] == "Local" ? "true" : "false")
    .WithEfSettings(builder.Configuration, "Directories", "Organisations")
    .WithPublicApiSecretEncryptionSettings(builder.Configuration)
    .WithExternalIdClientSettings(builder.Configuration)
    .WithRedisConnectionStrings(dotnetRedisConnectionString, "GeneralRedisCache", "BullMQ")
    .WithEnvironment("GeneralRedisCache__DatabaseNumber", builder.Configuration["GeneralRedisCache:DatabaseNumber"])
    .WithGovNotifySettings(builder.Configuration);

if (dotNetComponents.GetValue("ProfileEnabled", true)) {
    builder.AddDotnetPlatformApp("profile", "../Dfe.SignIn.Web.Profile/Dfe.SignIn.Web.Profile.csproj")
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithServiceBusAuditingSettings(builder.Configuration)
        .WithInternalApiSettings(builder.Configuration)
        .WithLocalInternalApi(internalApi)
        .WithRedisConnectionStrings(dotnetRedisConnectionString, "GeneralRedisCache", "SessionRedisCache", "TokenRedisCache")
        .WithOidcSettings(builder.Configuration)
        .WithExternalIdSettings(builder.Configuration)
        .WithSessionSettings(builder.Configuration)
        .WaitFor(redis);
}

if (dotNetComponents.GetValue("PublicApiEnabled", true)) {
    builder.AddDotnetPlatformApp("public-api", "../Dfe.SignIn.PublicApi/Dfe.SignIn.PublicApi.csproj")
        .WithPlatformSettings(builder.Configuration)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WithServiceBusAuditingSettings(builder.Configuration)
        .WithInternalApiSettings(builder.Configuration)
        .WithLocalInternalApi(internalApi)
        .WithRedisConnectionStrings(dotnetRedisConnectionString, "SelectOrganisationSessionRedisCache", "InteractionsRedisCache")
        .WithBearerTokenSettings(builder.Configuration)
        .WithPublicApiSecretEncryptionSettings(builder.Configuration)
        .WithSelectOrganisationSettings(builder.Configuration)
        .WithEfSettings(builder.Configuration, "Organisations", "Directories", "Audit")
        .WaitFor(internalApi)
        .WaitFor(redis);
}

// Node applications
var nodeComponents = builder.Configuration.GetSection("Components:Node");
var nodeRootDir = builder.Configuration["NodePlatformDirectory"]
    ?? throw new InvalidOperationException("NodePlatformDirectory is not configured.");

var nodeEnvFileName = builder.Configuration["NodeEnvFileName"]
    ?? throw new InvalidOperationException("NodeEnvFileName is not configured.");

IResourceBuilder<JavaScriptAppResource>? oidc = null;
if (nodeComponents.GetValue("OidcEnabled", true)) {
    oidc = builder.AddNodePlatformApp("oidc", nodeRootDir, "login.dfe.oidc", 4436,
            nodeRedisConnectionString, envFileName: nodeEnvFileName, scriptName: "dev")
        .WithTags(ResourceTags.Node)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WaitFor(redis);
}

IResourceBuilder<JavaScriptAppResource>? interactor = null;
if (nodeComponents.GetValue("InteractionsEnabled", true)) {
    interactor = builder.AddNodePlatformApp("interactions", nodeRootDir, "login.dfe.interactions", 4431,
            nodeRedisConnectionString, envFileName: nodeEnvFileName, scriptName: "dev")
        .WithTags(ResourceTags.Node)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WaitForIfPresent(oidc);
}

if (nodeComponents.GetValue("ServicesEnabled", true)) {
    builder.AddNodePlatformApp("services", nodeRootDir, "login.dfe.services", 41012,
            nodeRedisConnectionString, envFileName: nodeEnvFileName)
        .WithTags(ResourceTags.Node)
        .WithFrontendAssets(frontend, builder.Configuration)
        .WaitForIfPresent(oidc)
        .WaitForIfPresent(interactor);
}

if (nodeComponents.GetValue("Run-TlsProxy", false)) {
    builder.AddExecutable("tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy")
        .WithTags(ResourceTags.Infrastructure);
}
#pragma warning restore ASPIRERESOURCETRAITS001
await builder.Build().RunAsync();
