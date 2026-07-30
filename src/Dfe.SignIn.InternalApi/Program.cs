using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Gateways.EntityFramework.Configuration;
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
#if !DEBUG // Exclude when debugging locally.
    .AddJwtBearer(options => {
        var section = builder.Configuration.GetRequiredSection("AzureAd");
        options.Audience = section.GetValue<string>("Audience");
        options.MetadataAddress = section.GetValue<string>("Instance") + "/" + section.GetValue<string>("TenantId") + "/.well-known/openid-configuration";
    })
#endif
;

var authorizationBuilder = builder.Services.AddAuthorizationBuilder();
authorizationBuilder.SetFallbackPolicy(
    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
);
#if DEBUG // Include when debugging locally.
if (builder.Environment.IsEnvironment("Local")) {
    authorizationBuilder.SetDefaultPolicy(
        new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build()
    );
}
#endif

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

if (builder.Environment.IsEnvironment("Local")) {
    builder.Services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();
}
else {
    builder.Services.AddAuditingWithServiceBus(builder.Configuration);
}

builder.Services.AddFeaturesServices();
builder.Services.AddValidatorsFromAssemblyContaining<CoreContractsMarker>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseLogContextEnrichment();

app.UseMiddleware<CancellationContextMiddleware>();
app.UseDsiSecurityHeaderPolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();

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
