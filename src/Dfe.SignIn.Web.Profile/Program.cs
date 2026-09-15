using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache;
using Dfe.SignIn.Gateways.Entra;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.Web.Profile;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// --- Host / infrastructure ---
builder.AddServiceDefaults(["/v2/healthcheck"]);

if (builder.Environment.IsEnvironment("Local")) {
    builder.Configuration.AddUserSecrets<Program>();
}

builder.WebHost.ConfigureKestrel((context, options) => {
    options.AddServerHeader = false;
    context.Configuration.GetSection("Kestrel").Bind(options);
});

// Legacy AzureMonitor section — overlaps with AddServiceDefaults + APPLICATIONINSIGHTS_CONNECTION_STRING.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- Auth / session ---
builder.Services
    .AddUserSessions(builder.Configuration)
    .AddDsiAuthentication(builder.Configuration)
    .AddExternalAuthentication(builder.Configuration)
    .AddAuthorization(options => options.AddDsiPolicies())
    .AddDsiAuthorizationHandlers();

// --- MVC / web framework ---
builder.Services
    .AddControllersWithViews()
    .AddDsiMvcExtensions();

builder.Services.ConfigureDsiAntiforgeryCookie();

builder.Services.ConfigureDfeSignInJsonSerializerOptions();
// .AddInteractionFramework(); // not needed — Profile no longer uses IInteractionDispatcher / interactors

// --- Profile app services ---
builder.Services
    .AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>()
    .AddScoped<IServiceNavigationBuilder, ServiceNavigationBuilder>();

// --- Credentials ---
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient"));

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(azureTokenCredentialOptions);
var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

// --- Data protection ---
builder.Services
    // .Configure<InternalApiClientOptions>(...); // not needed here — moved into another extension (with AddUsersApiClient)
    // .SetupInternalApiClient(tokenCredential); // not needed — Profile uses Refit IUsersApiClient only
    // .SetupResiliencePipelines(builder.Configuration); // not needed — only used by SetupInternalApiClient / Node clients
    .AddDsiDataProtection(builder.Configuration, azureTokenCredential, typeof(Program).Assembly.GetName().Name!);

// --- Caching ---
// not needed — nothing in Profile uses GeneralCache (session + Entra token caches registered elsewhere)
// builder.Services.SetupRedisCacheStore(
//     DistributedCacheKeys.GeneralCache,
//     builder.Configuration.GetRequiredSection("GeneralRedisCache"));

// --- Auditing ---
builder.Services
    .SetupAuditContext()
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential)
    .AddAuditingWithServiceBus(builder.Configuration, builder.Environment);

// --- Options / frontend ---
builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));

builder.Services.SetupFrontendAssets();

// --- API clients / Entra ---
builder.Services
    .AddHttpContextAccessor() // redundant with SetupAuditContext (TryAdd) — fine to leave
    .AddEntraDelegatedServices()
    .AddUsersApiClient(tokenCredential);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// TEMP: Add fake interactor implementations.
// not needed — no interactors in Profile
// builder.Services.AddInteractors(InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly));

// --- Local-only overrides ---
// Allow self-signed IdP certificates on the OIDC backchannel in Local only.
if (builder.Environment.IsEnvironment("Local")) {
    builder.Services.PostConfigure<OpenIdConnectOptions>(
        OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.BackchannelHttpHandler = new HttpClientHandler {
                ServerCertificateCustomValidationCallback = // NOSONAR
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });
}

var app = builder.Build();

// --- Pipeline ---
// app.UseMiddleware<CancellationContextMiddleware>(); // not needed — only used with interaction framework

app.UseDsiSecurityHeaderPolicy();

if (!app.Environment.IsEnvironment("Local")) {
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
    // app.UseHttpsRedirection(); // not needed here — called once below after UseForwardedHeaders
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseHealthChecks();
app.UseLogContextEnrichment();

app.UseRewriter(new RewriteOptions().AddRedirect("(.*)/$", "$1", statusCode: 301));

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

/// <summary>
/// The entry point class for the application (exposed for integration tests).
/// </summary>
internal sealed partial class Program { }
