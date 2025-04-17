using Azure.Monitor.OpenTelemetry.AspNetCore;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Configuration.Interactions;
using Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Add services to the container.
builder.Services
    .Configure<ApplicationOptions>(builder.Configuration.GetRequiredSection("Application"));
builder.Services
    .Configure<BearerTokenOptions>(builder.Configuration.GetRequiredSection("BearerToken"));
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Access, NodeApiName.Applications, NodeApiName.Organisations]);

builder.Services.SetupEndpoints();
builder.Services.SetupSwagger();
builder.Services.SetupAutoMapper();
builder.Services.SetupScopedSession();
builder.Services.SetupHealthChecks(
    builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache")
);

builder.Services
    .SetupRedisSessionStore(builder.Configuration.GetRequiredSection("SelectOrganisationSessionRedisCache"))
    .Configure<SelectOrganisationOptions>(builder.Configuration.GetRequiredSection("SelectOrganisation"))
    .SetupSelectOrganisationInteractions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction()) {
    app.UseSwagger(c => {
        // Override the default route template.
        c.RouteTemplate = "v2/swagger.json";
    });
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/v2/swagger.json", "Dfe.SignIn.PublicApi v1");
        c.RoutePrefix = "v2/swagger"; // Updates the Swagger UI path
    });
}

app.UseHttpsRedirection();
app.UseHealthChecks();
app.UseBearerTokenAuthMiddleware();

app.UseDigitalSigningEndpoints();
app.UseSelectOrganisationEndpoints();

app.Run();

/// <exclude/>
[ExcludeFromCodeCoverage]
public partial class Program { }
