using Azure.Identity;
using Dfe.SignIn.Gateways.Entra;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.Web.Profile;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Extensions;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Host / infrastructure
builder.AddServiceDefaults(["/v2/healthcheck"]);

if (builder.Environment.IsLocal()) {
    builder.Configuration.AddUserSecrets<Program>();
}

builder.WebHost.ConfigureKestrel((context, options) => {
    options.AddServerHeader = false;
    context.Configuration.GetSection("Kestrel").Bind(options);
});

builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Auth / session
builder.Services
    .AddUserSessions(builder.Configuration)
    .AddDsiAuthentication(builder.Configuration)
    .AddExternalAuthentication(builder.Configuration)
    .AddAuthorization(options => options.AddDsiPolicies())
    .AddDsiAuthorizationHandlers();

// MVC / web framework
builder.Services
    .AddControllersWithViews()
    .AddDsiMvcExtensions();

builder.Services
    .ConfigureDsiAntiforgeryCookie();

// Profile app services
builder.Services
    .AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>()
    .AddScoped<IServiceNavigationBuilder, ServiceNavigationBuilder>();

// Credentials
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient"));

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration
    .GetSection("Azure")
    .Bind(azureTokenCredentialOptions);

var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

// Data protection
builder.Services
    .AddDsiDataProtection(builder.Configuration, azureTokenCredential, typeof(Program).Assembly.GetName().Name!);

// Auditing
builder.Services
    .AddDsiAuditing(builder.Configuration, azureTokenCredential, builder.Environment);

// Options / frontend
builder.Services
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"))
    .AddOptionsWithValidation<PlatformSettings>();

builder.Services
    .AddFrontendAssets();

// API clients / Entra
builder.Services
    .AddEntraDelegatedServices()
    .AddUsersApiClient(tokenCredential);

// Local-only overrides
// Allow self-signed IdP certificates on the OIDC backchannel in Local only.
if (builder.Environment.IsLocal()) {
    builder.Services.PostConfigure<OpenIdConnectOptions>(
        OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.BackchannelHttpHandler = new HttpClientHandler {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // NOSONAR
            };
        });
}

var app = builder.Build();

// Pipeline
app.UseExceptionHandler("/Error/Index");
if (!app.Environment.IsLocal()) {
    app.UseHsts();
}

app.UseDsiSecurityHeaderPolicy();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseHealthChecks();
app.UseLogContextEnrichment();

app.UseRewriter(new RewriteOptions().AddRedirect("(.*)/$", "$1", statusCode: 301))
   .UseRouting();

app.UseAuthentication()
   .UseMiddleware<UserProfileMiddleware>()
   .UseStatusCodePagesWithReExecute("/Error", "?code={0}")
   .UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}"
);

await app.RunAsync();

/// <summary>
/// The entry point class for the application (exposed for integration tests).
/// </summary>
internal sealed partial class Program { }
