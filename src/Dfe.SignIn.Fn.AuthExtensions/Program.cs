using Azure.Identity;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Fn.AuthExtensions.Configuration;
using Dfe.SignIn.Gateways.ServiceBus;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.UseMiddleware((context, next) => {
    var cancellationContext = context.InstanceServices.GetRequiredService<ICancellationContext>();
    cancellationContext.CancellationToken = context.CancellationToken;
    return next();
});

builder.Configuration
    .AddJsonFile("appsettings.json")
#if DEBUG
    .AddUserSecrets<Program>()
#endif
    .AddEnvironmentVariables();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddInteractionFramework();

builder.Services
    .Configure<AuditOptions>(builder.Configuration.GetSection("Audit"))
    .Configure<AuditOptions>(options => options.ApplicationName ??= "AuthExtensions")
    .SetupAuditContext();

var azureTokenCredentialOptions = new DefaultAzureCredentialOptions();
builder.Configuration.GetSection("Azure").Bind(azureTokenCredentialOptions);
var azureTokenCredential = new DefaultAzureCredential(azureTokenCredentialOptions);

builder.Services
    .AddServiceBusIntegration(builder.Configuration, azureTokenCredential)
    .AddAuditingWithServiceBus(builder.Configuration);

// Get token credential for making API requests to Node APIs.
var apiTokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions")
);

IEnumerable<NodeApiName> requiredNodeApiNames = [
    NodeApiName.Access,  NodeApiName.Directories,  NodeApiName.Organisations, NodeApiName.Search];

builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient(requiredNodeApiNames, apiTokenCredential)
    .SetupResilientHttpClient(requiredNodeApiNames.Select(api => api.ToString()), builder.Configuration, "NodeApiDefault");

builder.Services
    .Configure<BlockedEmailAddressOptions>(options => {
        var section = builder.Configuration.GetSection("BlockedEmailAddresses");
        options.BlockedDomains = section.GetJsonList("BlockedDomains");
        options.BlockedNames = section.GetJsonList("BlockedNames");
    })
    .AddInteractor<CheckIsBlockedEmailAddressUseCase>()
    .AddInteractor<AutoLinkEntraUserToDsiUseCase>();

await builder.Build().RunAsync();
