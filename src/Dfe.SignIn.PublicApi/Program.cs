using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.NodeApiClient;
using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Configuration.Interactions;
using Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services
    .Configure<ApplicationOptions>(builder.Configuration.GetRequiredSection("Application"));
builder.Services
    .Configure<BearerTokenOptions>(builder.Configuration.GetRequiredSection("BearerToken"));
builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([NodeApiName.Applications, NodeApiName.Access, NodeApiName.Organisations]);

builder.Services.SetupEndpoints();
builder.Services.SetupSwagger();
builder.Services.SetupAutoMapper();
builder.Services.SetupScopedSession();

builder.Services.SetupSelectOrganisationInteractions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseBearerTokenAuthMiddleware();

app.UseDigitalSigningEndpoints();
app.UseSelectOrganisationEndpoints();

app.Run();

/// <exclude/>
[ExcludeFromCodeCoverage]
public partial class Program { }
