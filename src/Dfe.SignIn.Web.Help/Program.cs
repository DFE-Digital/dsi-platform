using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.Help.Configuration;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

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

// Get token credential for making API requests to internal APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

IEnumerable<NodeApiName> requiredNodeApiNames = [
    NodeApiName.Applications];

// Add services to the container.
builder.Services.AddControllersWithViews().AddDsiMvcExtensions();
builder.Services.ConfigureDsiAntiforgeryCookie();
builder.Services.AddHealthChecks();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .AddInteractionFramework();

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.InteractionRequests,
        builder.Configuration.GetRequiredSection("InteractionsRedisCache"));

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
    .SetupNodeApiClient(requiredNodeApiNames, builder.Configuration.GetRequiredSection("InternalApiClient"), tokenCredential)
    .SetupResiliencePipelines(builder.Configuration)
    .AddDistributedInteractionCache<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>(options => {
        options.DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    });

builder.Services.SetupContentProcessing();

builder.Services.AddSingleton<IServiceNavigationBuilder, ServiceNavigationBuilder>();

builder.Services
    .Configure<GovNotifyOptions>(builder.Configuration.GetRequiredSection("GovNotify"))
    .AddGovNotify()
    .AddInteractor<SendEmailNotificationWithGovNotifyUseCase>();

builder.Services
    .Configure<RaiseSupportTicketByEmailOptions>(builder.Configuration.GetRequiredSection("RaiseSupportTicketByEmail"))
    .AddOptions<RaiseSupportTicketByEmailOptions>()
        .Configure<IOptions<PlatformOptions>>((options, platformOptionsAccessor) => {
            options.ContactUrl = platformOptionsAccessor.Value.ContactUrl;
        });
builder.Services
    .AddInteractor<GetSubjectOptionsForSupportTicketUseCase>()
    .AddInteractor<RaiseSupportTicketByEmailUseCase>();

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

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseHealthChecks();

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRedirect("(.*)/$", "$1", statusCode: 301);
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}"
);

if (app.Environment.IsEnvironment("Local")) {
    app.MapControllerRoute(
        name: "reloadTopics",
        pattern: "reload",
        defaults: new { controller = "Topic", action = "Reload" }
    );
}

app.MapControllerRoute(
    name: "topic",
    pattern: "{*url}",
    defaults: new { controller = "Topic", action = "Index" }
);

// Force content to be rendered before starting up the server.
await app.Services.GetRequiredService<ITopicIndexAccessor>()
    .GetIndexAsync();

await app.RunAsync();
