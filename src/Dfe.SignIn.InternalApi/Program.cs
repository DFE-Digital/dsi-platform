using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.EntityFramework.Configuration;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.WebFramework.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

if (builder.Environment.IsEnvironment("Local")) {
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.AddEnvironmentVariables();

// Add OpenTelemetry and configure it to use Azure Monitor when connection details are available.
if (builder.Configuration.GetSection("AzureMonitor").Exists()
    && !string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Add services to the container.
builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .ConfigureDfeSignInJsonSerializerOptions();

builder.Services.AddSwagger();
builder.Services.AddHealthChecks();
builder.Services.AddGlobalExceptionHandler();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        var section = builder.Configuration.GetSection("AzureAd");
        var audience = section.GetValue<string>("Audience")
                       ?? builder.Configuration["InternalApiClient:Resource"]
                       ?? builder.Configuration["InternalApiClient:ClientId"];
        var instance = section.GetValue<string>("Instance")
                       ?? builder.Configuration["InternalApiClient:HostUrl"]
                       ?? "https://login.microsoftonline.com";
        var tenantId = section.GetValue<string>("TenantId")
                       ?? builder.Configuration["InternalApiClient:Tenant"];

        options.Audience = audience;
        options.MetadataAddress = $"{instance.TrimEnd('/')}/{tenantId}/.well-known/openid-configuration";
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(
        new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
    );

IEnumerable<NodeApiName> requiredNodeApiNames = [NodeApiName.Search];

// Get token credential for making API requests to internal APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

builder.Services
    .Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApiClient"))
    .SetupNodeApiClient(requiredNodeApiNames, builder.Configuration.GetRequiredSection("InternalApiClient"), tokenCredential)
    .SetupResiliencePipelines(builder.Configuration);

builder.Services
    .AddInteractionFramework()
    .AddInteractionCaching(builder.Configuration);

builder.Services
    .AddApplicationUseCases(builder.Configuration)
    .AddOrganisationUseCases(builder.Configuration)
    .AddPublicApiUseCases(builder.Configuration)
    .AddSupportTicketUseCases(builder.Configuration)
    .AddUserUseCases(builder.Configuration);

builder.Services
    .AddEntityFrameworkServices(
        builder.Configuration.GetRequiredSection("EntityFramework"),
        addDirectories: true,
        addOrganisations: true,
        addAudit: false
    );

builder.Services
    .Configure<AuditOptions>(builder.Configuration.GetRequiredSection("Audit"))
    .SetupAuditContext();

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(azureTokenCredentialOptions);
var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential);

//todo: remove this once we have migrated all the code away from using the WriteToAuditInteractor to using the ServiceBusAuditInteractor.
//This is only needed for local development, as the ServiceBusAuditInteractor will not work locally.
if (builder.Environment.IsEnvironment("Local")) {
    builder.Services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();
}

builder.Services.AddAuditingWithServiceBus(builder.Configuration, builder.Environment);

builder.Services
    .AddGovNotify(builder.Configuration)
    .SetupRedisCacheStore(DistributedCacheKeys.GeneralCache, builder.Configuration.GetRequiredSection("GeneralRedisCache"))
    .AddFeaturesServices(builder.Configuration)
    .AddValidatorsFromAssemblyContaining<CoreContractsMarker>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseLogContextEnrichment();

app.UseMiddleware<CancellationContextMiddleware>();
app.UseDsiSecurityHeaderPolicy();

app.UseSwagger();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseHealthChecks();

app.UseApplicationEndpoints();
app.UseOrganisationEndpoints();
app.UsePublicApiEndpoints();
app.UseSupportTicketEndpoints();
app.UseUserEndpoints();

app.MapFeaturesEndpoints();

await app.RunAsync();

// Expose the Program class to the integration tests project
/// <summary>
/// The entry point class for the application.
/// </summary>
public partial class Program { }
