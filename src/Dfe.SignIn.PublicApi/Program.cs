using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Dfe.SignIn.WebFramework.Configuration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Local")) {
    builder.Configuration.AddUserSecrets<Program>();
}
builder.Configuration.AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel((context, options) => {
    options.AddServerHeader = false;
    context.Configuration.GetSection("Kestrel").Bind(options);
});

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Get token credential for making API requests to Node APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions")
);

IEnumerable<NodeApiName> requiredNodeApiNames = [
    NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations];

// Add services to the container.
builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .Configure<BearerTokenOptions>(builder.Configuration.GetRequiredSection("BearerToken"));
builder.Services
    .Configure<AuditOptions>(builder.Configuration.GetRequiredSection("Audit"))
    .SetupAuditContext();
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient(requiredNodeApiNames, tokenCredential)
    .SetupResilientHttpClient(requiredNodeApiNames.Select(api => api.ToString()), builder.Configuration, "NodeApiDefault");

builder.Services.SetupEndpoints();
builder.Services.SetupSwagger();
builder.Services.SetupScopedSession();
builder.Services.AddHealthChecks();

builder.Services
    .AddInteractionFramework()
    .AddInteractionCaching(builder.Configuration);

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(azureTokenCredentialOptions);
var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential);

if (builder.Environment.IsEnvironment("Local")) {
    builder.Services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();
}
else {
    builder.Services.AddAuditingWithServiceBus(builder.Configuration);
}

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.SelectOrganisationSessions,
        builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .AddSelectOrganisationSessionCache()
    .Configure<SelectOrganisationOptions>(builder.Configuration.GetRequiredSection("SelectOrganisation"))
    .SetupSelectOrganisationInteractions();

builder.Services.SetupApiSecretEncryption(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CancellationContextMiddleware>();
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
