using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
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

// Get token credential for making API requests to Node APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions")
);

// Add services to the container.
builder.Services.AddControllersWithViews().AddDsiMvcExtensions();
builder.Services.AddHealthChecks();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .ConfigureExternalModelJsonSerialization();

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
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations], tokenCredential);

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.SelectOrganisationSessions,
        builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .AddSelectOrganisationSessionCache()
    .SetupSelectOrganisationInteractions();

builder.Services
    .AddInteractionFramework();

// TEMP: Add mocked interactors.
builder.Services.AddInteractors(
    InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly)
);

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
