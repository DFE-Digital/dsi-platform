using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Dfe.SignIn.WebFramework.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
#if DEBUG
    .AddJsonFile("appsettings.Local.json")
    .AddUserSecrets<Program>()
#endif
    .AddEnvironmentVariables();

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Get token credential for making API requests to Node APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions")
);

// Add services to the container.
builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .Configure<BearerTokenOptions>(builder.Configuration.GetRequiredSection("BearerToken"));
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations], tokenCredential);

builder.Services.SetupEndpoints();
builder.Services.SetupSwagger();
builder.Services.SetupScopedSession();
builder.Services.AddHealthChecks();

builder.Services
    .AddInteractionFramework()
    .AddInteractionCaching(builder.Configuration);

var azureTokenCredential = new DefaultAzureCredential();
builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential);

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.SelectOrganisationSessions,
        builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .AddSelectOrganisationSessionCache()
    .Configure<SelectOrganisationOptions>(builder.Configuration.GetRequiredSection("SelectOrganisation"))
    .SetupSelectOrganisationInteractions();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDsiSecurityHeaderPolicy();

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("v1/swagger.json", "DfE Sign-in Public API");
});

app.UseHttpsRedirection();
app.UseHealthChecks();
app.UseBearerTokenAuthMiddleware();

app.UseSelectOrganisationEndpoints();
app.UseUserEndpoints();

await app.RunAsync();
