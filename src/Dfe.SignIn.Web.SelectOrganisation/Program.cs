using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Core.ExternalModels;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.Web.SelectOrganisation.Configuration;
using Dfe.SignIn.Web.SelectOrganisation.Configuration.Interactions;
using Dfe.SignIn.WebFramework.Configuration;

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

builder.Services
    .ConfigureDfeSignInJsonSerializerOptions()
    .ConfigureExternalModelJsonSerialization();

builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .Configure<AssetOptions>(builder.Configuration.GetRequiredSection("Assets"))
    .SetupFrontendAssets();
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations]);

builder.Services.SetupAutoMapper();

builder.Services
    .SetupHealthChecks(
        builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache")
    );

builder.Services
    .SetupRedisSessionStore(builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .SetupSelectOrganisationInteractions();

// TEMP: Add mocked interactors.
builder.Services.AddInteractors(
    InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly)
);

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
app.UseRouting();
app.UseHealthChecks();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) {
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

app.Run();

/// <exclude/>
[ExcludeFromCodeCoverage]
public partial class Program { }
