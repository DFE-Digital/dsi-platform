using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.Interfaces.Graph;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Rewrite;

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
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Directories:AuthenticatedHttpClientOptions")
);

builder.Services
    .AddUserSessions(builder.Configuration)
    .AddDsiAuthentication(builder.Configuration)
    .AddExternalAuthentication(builder.Configuration);

builder.Services
    .AddAuthorization(options => options.AddDsiPolicies())
    .AddDsiAuthorizationHandlers();

builder.Services.AddControllersWithViews().AddDsiMvcExtensions();
builder.Services.AddHealthChecks();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .AddInteractionFramework();

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.GeneralCache,
        builder.Configuration.GetRequiredSection("GeneralRedisCache"))
    .AddInteractionLimiter<InitiateChangeEmailAddressRequest>(builder.Configuration);

var azureTokenCredential = new DefaultAzureCredential();
builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential);

if (builder.Environment.IsEnvironment("Local")) {
    builder.Services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();
}
else {
    builder.Services.AddAuditingWithServiceBus(builder.Configuration);
}

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
    .SetupNodeApiClient([NodeApiName.Directories], tokenCredential);

builder.Services.Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApi"));
builder.Services.SetupInternalApiClient(tokenCredential);

builder.Services
    .Configure<GovNotifyOptions>(builder.Configuration.GetRequiredSection("GovNotify"))
    .AddGovNotify()
    .AddInteractor<SendEmailNotificationWithGovNotifyUseCase>();

builder.Services
    .AddHttpContextAccessor()
    .AddSingleton<IPersonalGraphServiceFactory, PersonalGraphServiceFactory>()
    .AddSingleton<IGraphApiChangeUserPassword, GraphApiChangeUserPassword>();

// TEMP: Add fake interactor implementations.
// builder.Services.AddInteractors(InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.UseMiddleware<CancellationContextMiddleware>();
app.UseDsiSecurityHeaderPolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsEnvironment("Local")) {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseHealthChecks();

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRedirect("(.*)/$", "$1", statusCode: 301);
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<UserProfileMiddleware>();
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}"
);

await app.RunAsync();
