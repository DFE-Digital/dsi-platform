using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.Help.Configuration;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration
    .AddJsonFile("appsettings.Local.json")
    .AddUserSecrets<Program>();
#endif

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Add services to the container.
builder.Services.AddControllersWithViews();
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
    .Configure<AssetOptions>(builder.Configuration.GetRequiredSection("Assets"))
    .SetupFrontendAssets();
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Applications])
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

app.UseDsiSecurityHeaderPolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHealthChecks();

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRedirect("(.*)/$", "$1", statusCode: 301);
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}"
);

#if DEBUG
app.MapControllerRoute(
    name: "reloadTopics",
    pattern: "reload",
    defaults: new { controller = "Topic", action = "Reload" }
);
#endif

app.MapControllerRoute(
    name: "topic",
    pattern: "{*url}",
    defaults: new { controller = "Topic", action = "Index" }
);

// Force content to be rendered before starting up the server.
await app.Services.GetRequiredService<ITopicIndexAccessor>()
    .GetIndexAsync();

await app.RunAsync();
