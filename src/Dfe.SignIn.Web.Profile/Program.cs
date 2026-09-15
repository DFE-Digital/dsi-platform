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

builder.AddServiceDefaults(["/v2/healthcheck"]);

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

builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
    .AddUserSessions(builder.Configuration)
    .AddDsiAuthentication(builder.Configuration)
    .AddExternalAuthentication(builder.Configuration)
    .AddAuthorization(options => options.AddDsiPolicies())
    .AddDsiAuthorizationHandlers();

builder.Services
    .AddControllersWithViews()
    .AddDsiMvcExtensions();

builder.Services
    .ConfigureDsiAntiforgeryCookie();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions();
//.AddInteractionFramework()

builder.Services
    .AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>()
    .AddScoped<IServiceNavigationBuilder, ServiceNavigationBuilder>();

// Get token credential for making API requests to internal APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(azureTokenCredentialOptions);
var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

builder.Services
    //.Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApiClient"))
    //.SetupInternalApiClient(tokenCredential)
    //.SetupResiliencePipelines(builder.Configuration)
    .AddDsiDataProtection(builder.Configuration, azureTokenCredential, typeof(Program).Assembly.GetName().Name!);

builder.Services
    .SetupRedisCacheStore(DistributedCacheKeys.GeneralCache,
        builder.Configuration.GetRequiredSection("GeneralRedisCache"));

builder.Services
    .SetupAuditContext()
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential)
    .AddAuditingWithServiceBus(builder.Configuration, builder.Environment);

builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));

builder.Services
    .SetupFrontendAssets();

builder.Services
    .AddHttpContextAccessor()
    .AddEntraDelegatedServices()
    .AddUsersApiClient(tokenCredential);

builder.Services
    .AddValidatorsFromAssemblyContaining<Program>();

// TEMP: Add fake interactor implementations.
// builder.Services.AddInteractors(InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly));

// In local development, disable SSL certificate validation for the OpenID Connect backchannel to allow using self-signed certificates.
// This should only be used in the Local environment and not in any other environment to avoid security risks.
// Note: This is necessary because the OpenID Connect middleware makes backchannel HTTP requests to the identity provider for token validation and other operations, and in local development,
// the identity provider may be using a self-signed certificate that is not trusted by the development machine.
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

//app.UseMiddleware<CancellationContextMiddleware>();
app.UseDsiSecurityHeaderPolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsEnvironment("Local")) {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    //app.UseHttpsRedirection();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseHealthChecks();
app.UseLogContextEnrichment();

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

// Expose the Program class to the integration tests project
/// <summary>
/// The entry point class for the application.
/// </summary>
internal sealed partial class Program { }
