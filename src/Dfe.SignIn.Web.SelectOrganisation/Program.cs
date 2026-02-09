using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.SelectOrganisation.Configuration;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Configuration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Local")) {
    builder.Configuration.AddUserSecrets<Program>();
}

builder.WebHost.ConfigureKestrel((context, options) => {
    options.AddServerHeader = false;
    context.Configuration.GetSection("Kestrel").Bind(options);
});

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Add services to the container.
builder.Services.AddControllersWithViews().AddDsiMvcExtensions();
builder.Services.ConfigureDsiAntiforgeryCookie();
builder.Services.AddHealthChecks();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .ConfigureExternalModelJsonSerialization();

builder.Services
    .AddInteractionFramework();

IEnumerable<NodeApiName> requiredNodeApiNames = [
    NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations];

// Get token credential for making API requests to internal APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

builder.Services
    .Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApiClient"))
    .SetupInternalApiClient(tokenCredential)
    .SetupNodeApiClient(requiredNodeApiNames, builder.Configuration.GetRequiredSection("InternalApiClient"), tokenCredential)
    .SetupResiliencePipelines(builder.Configuration);

builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .Configure<AuditOptions>(builder.Configuration.GetRequiredSection("Audit"))
    .SetupAuditContext();
builder.Services
    .Configure<AssetOptions>(builder.Configuration.GetRequiredSection("Assets"))
    .SetupFrontendAssets();

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.SelectOrganisationSessions,
        builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .AddSelectOrganisationSessionCache()
    .SetupSelectOrganisationInteractions();

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

var app = builder.Build();

app.UseMiddleware<CancellationContextMiddleware>();
app.UseDsiSecurityHeaderPolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsEnvironment("Local")) {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseRouting();
app.UseHealthChecks();
app.UseAuthorization();

if (app.Environment.IsEnvironment("Local")) {
    app.MapControllerRoute(
        name: "developerTool",
        pattern: "{controller=Developer}/{action=Index}/{id?}"
    );
}

app.MapControllerRoute(
    name: "selectOrganisation",
    pattern: "{clientId}/{sessionKey}",
    defaults: new { controller = "SelectOrganisation", action = "Index" }
);

await app.RunAsync();
