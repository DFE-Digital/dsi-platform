using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApiClient;
using Dfe.SignIn.SelectOrganisation.Data.DistributedCache;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Signing;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "localhost:6379";
});

builder.Services.AddNodeApiClient([NodeApiName.Access], options => { });

builder.Services.AddSelectOrganisationSessionCache(options => { });
builder.Services.AddApplication(options => { });
builder.Services.AddFrontendAssets(options => { });

// TEMP: Add mocked interactors.
builder.Services.AddInteractors(
    InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(typeof(Program).Assembly)
);
builder.Services.Configure<DefaultCallbackPayloadSignerOptions>(options => {
    using var rsa = new RSACryptoServiceProvider(2048);
    options.Algorithm = HashAlgorithmName.SHA256;
    options.KeyId = "3605fbcf-7664-4e9f-aecc-a7d1ae7b175e";
    options.Padding = RSASignaturePadding.Pkcs1;
    options.PrivateKeyPem = rsa.ExportRSAPrivateKeyPem();
});
builder.Services.AddSingleton<ICallbackPayloadSigner, DefaultCallbackPayloadSigner>();

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
