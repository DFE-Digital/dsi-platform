using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Web.Help.Configuration;
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
    .ConfigureDfeSignInJsonSerializerOptions();

builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .Configure<AssetOptions>(builder.Configuration.GetRequiredSection("Assets"))
    .SetupFrontendAssets();

builder.Services.SetupAutoMapper();
builder.Services.SetupHealthChecks();
builder.Services.SetupContentProcessing();

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

app.Run();

/// <exclude/>
[ExcludeFromCodeCoverage]
public partial class Program { }
