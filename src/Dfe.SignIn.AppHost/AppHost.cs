using System.Reflection;
using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);

var redis = builder.AddRedis("infra-redis")
    .WithDataVolume()
    // Uncomment .WithRedisInsight() for advanced memory analysis, active profilers, and module reporting.
    // .WithRedisInsight()
    // Defaults to Redis Commander for a fast, lightweight cache viewer that uses minimal resources.
    .WithRedisCommander();

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
var redisConnectionString = redis.Resource.ConnectionStringExpression;

builder.AddProject<Projects.Dfe_SignIn_Web_Help>("app-help", launchProfileName: "http")
    .WithSharedConfiguration(builder.Configuration, frontend.GetEndpoint("http"))
    .WithEnvironment("InteractionsRedisCache__ConnectionString", redisConnectionString)
    .WithEnvironment("GovNotify__ApiKey", govNotifyConfig["ApiKey"])
    .WithEnvironment("RaiseSupportTicketByEmail__SupportEmailAddress", supportEmailConfig["SupportEmailAddress"])
    .WithEnvironment("RaiseSupportTicketByEmail__EmailTemplateId", supportEmailConfig["EmailTemplateId"])
    .WaitFor(frontend)
    .WaitFor(redis);

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

builder.AddExecutable("tool-tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy");

builder.Build().Run();
