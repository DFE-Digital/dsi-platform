using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApiClient;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Configuration.Interactions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.SetupPublicApiSigningInteractions();
builder.Services.SetupSelectOrganisationInteractions();

builder.Services.AddNodeApiClient([NodeApiName.Access], options => { });

builder.Services.AddApplication(options => { });
builder.Services.AddFrontendAssets(options => { });

// TEMP: Add mocked interactors.
builder.Services.AddInteractors(
    InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

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

[ExcludeFromCodeCoverage]
public partial class Program { }
