using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.NodeApiClient;
using Dfe.SignIn.PublicApi.Endpoints;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

builder.Services.AddNodeApiClient(options => {
    options.Apis = [
        new NodeApiOptions {
            ApiName = NodeApiName.Applications,
            BaseAddress = new Uri(""),
            AuthenticatedHttpClientOptions = new NodeApiAuthenticatedHttpClientOptions {
                 ClientId = Guid.Parse(""),
                 ClientSecret = "",
                 HostUrl = new Uri(""),
                 Resource = Guid.Parse(""),
                 Tenant = "",
                 ProxyUrl = new Uri(""),
                 UseProxy = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Debugger.IsAttached
            }
        },
        new NodeApiOptions {
            ApiName = NodeApiName.Directories,
            BaseAddress = new Uri(""),
            AuthenticatedHttpClientOptions = new NodeApiAuthenticatedHttpClientOptions {
                 ClientId = Guid.Parse(""),
                 ClientSecret = "",
                 HostUrl = new Uri(""),
                 Resource = Guid.Parse(""),
                 Tenant = "",
                 ProxyUrl = new Uri(""),
                 UseProxy = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Debugger.IsAttached
            }
        }
    ];
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseBearerTokenAuthMiddleware(options => { });

app.RegisterSelectOrganisationEndpoints();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }