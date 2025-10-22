using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Gateways.GovNotify;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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

// Get token credential for making API requests to Node APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Directories:AuthenticatedHttpClientOptions")
);

builder.Services
    .AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(options => {
        options.CallbackPath = new PathString("/auth/cb");
        options.SignedOutCallbackPath = new PathString("/signout/complete");

        builder.Configuration.GetRequiredSection("Oidc").Bind(options);

        options.SaveTokens = true;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.Events.OnSignedOutCallbackRedirect = async context => {
            var platformOptions = context.HttpContext.RequestServices
                .GetRequiredService<IOptionsMonitor<PlatformOptions>>().CurrentValue;
            context.Response.Redirect(platformOptions.ServicesUrl.ToString());
        };
    })
    .AddCookie(options => {
        options.Cookie.Name = "Session";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddSingleton(sp =>
    ConfidentialClientApplicationBuilder.Create(builder.Configuration.GetValue<string>("ExternalId:ClientId"))
        .WithClientSecret(builder.Configuration.GetValue<string>("ExternalId:ClientSecret"))
        .WithAuthority(builder.Configuration.GetValue<string>("ExternalId:CloudInstance"), builder.Configuration.GetValue<Guid>("ExternalId:TenantId"))
        .WithRedirectUri("http://localhost:41011/auth/callback")
        .Build()
);

var azureTokenCredential = new DefaultAzureCredential();
builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential)
    .AddAuditingWithServiceBus(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.AddTrimStringModelBinding());
builder.Services.AddHealthChecks();
builder.Services.AddSession();

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .AddInteractionFramework();

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

builder.Services
    .Configure<GovNotifyOptions>(builder.Configuration.GetRequiredSection("GovNotify"))
    .AddGovNotify()
    .AddInteractor<SendEmailNotificationWithGovNotifyUseCase>();

builder.Services
    .AddHttpContextAccessor()
    .AddScoped<IHybridAuthentication, HybridAuthentication>()
    .AddSingleton<IPersonalGraphServiceFactory, PersonalGraphServiceFactory>();

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
app.UseSession();

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRedirect("(.*)/$", "$1", statusCode: 301);
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}"
);

await app.RunAsync();
