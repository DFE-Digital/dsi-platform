using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.EntityFramework.Configuration;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features;
using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Extensions;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

PasswordChecker.Initialize();

if (builder.Environment.IsLocal()) {
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
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions();

builder.Services
    .AddOptionsWithValidation<PlatformSettings>()
    .AddOptionsWithValidation<EmailRestrictionsSettings>()
    .AddOptionsWithValidation<NotificationSettings>();

builder.Services.AddSwagger();
builder.Services.AddHealthChecks();
builder.Services
    .AddGlobalExceptionHandler()
    .AddInternalApiAuthentication(builder.Configuration, builder.Environment);

// Transitional Search integration: retain until the Node Search client is replaced.
IEnumerable<NodeApiName> requiredNodeApiNames = [NodeApiName.Search];
var searchApiCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

builder.Services
    .Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApiClient"))
    .SetupNodeApiClient(requiredNodeApiNames, builder.Configuration.GetRequiredSection("InternalApiClient"), searchApiCredential)
    .SetupResiliencePipelines(builder.Configuration);

// Temporary: remove after legacy callers and feature-internal dispatches are migrated.
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

var serviceBusCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(serviceBusCredentialOptions);
var serviceBusCredential = new DefaultAzureCredential(serviceBusCredentialOptions);

builder.Services
    .AddDsiAuditing(builder.Configuration, serviceBusCredential, builder.Environment);

builder.Services
    .AddMessagingServices();

builder.Services
    .AddGovNotify(builder.Configuration);

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.GeneralCache, builder.Configuration.GetRequiredSection("GeneralRedisCache"));

// Feature-local endpoint services and request validators.
builder.Services
    .AddFeaturesServices(builder.Configuration);

builder.Services
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

// Temporary: interaction-backed HTTP endpoints.
app.UseApplicationEndpoints();
app.UseOrganisationEndpoints();
app.UsePublicApiEndpoints();
app.UseSupportTicketEndpoints();
app.UseUserEndpoints();

// Migrated feature endpoints.
app.MapFeaturesEndpoints();

await app.RunAsync();

// Expose the Program class to the integration tests project
/// <summary>
/// The entry point class for the application.
/// </summary>
public partial class Program { }
