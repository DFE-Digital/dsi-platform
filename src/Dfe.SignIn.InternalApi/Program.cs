using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
#if DEBUG
    .AddJsonFile("appsettings.Local.json")
    .AddUserSecrets<Program>()
#endif
    .AddEnvironmentVariables();

// Add OpenTelemetry and configure it to use Azure Monitor.
if (builder.Configuration.GetSection("AzureMonitor").Exists()) {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// Add services to the container.
builder.Services
    .Configure<PlatformOptions>(builder.Configuration.GetRequiredSection("Platform"))
    .Configure<SecurityHeaderPolicyOptions>(builder.Configuration.GetSection("SecurityHeaderPolicy"));
builder.Services
    .ConfigureDfeSignInJsonSerializerOptions();

builder.Services.SetupSwagger();
builder.Services.AddHealthChecks();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
#if !DEBUG // Exclude when debugging locally.
    .AddJwtBearer(options => {
        var section = builder.Configuration.GetRequiredSection("AzureAd");
        options.Audience = section.GetValue<string>("Audience");
        options.MetadataAddress = section.GetValue<string>("Instance") + "/" + section.GetValue<string>("TenantId") + "/.well-known/openid-configuration";
    })
#endif
;

builder.Services.AddAuthorizationBuilder()
#if DEBUG // Include when debugging locally.
    .SetDefaultPolicy(
        new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build()
    )
#endif
;

builder.Services
    .AddInteractionFramework()
    .AddInteractionCaching(builder.Configuration)
    .AddUseCasesUser(builder.Configuration);

// Setup Service Bus integration if needed...
// var azureTokenCredential = new DefaultAzureCredential();
// builder.Services
//     .AddServiceBusIntegration(builder.Configuration, azureTokenCredential);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDsiSecurityHeaderPolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("v1/swagger.json", "DfE Sign-in Internal API");
});

app.UseHttpsRedirection();
app.UseHealthChecks();

app.UseUserEndpoints();

await app.RunAsync();
