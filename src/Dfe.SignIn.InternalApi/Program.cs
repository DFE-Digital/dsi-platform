using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.WebFramework.Configuration;

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

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("v1/swagger.json", "DfE Sign-in Internal API");
});

app.UseHttpsRedirection();
app.UseHealthChecks();

app.UseUserEndpoints();

await app.RunAsync();
