using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.NodeApi.Client;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
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
    .Configure<AuditOptions>(builder.Configuration.GetRequiredSection("Audit"))
    .Configure<AuditOptions>(options => options.ApplicationName ??= "AuthExtensions")
    .SetupAuditContext();

// Get token credential for making API requests to Node APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions")
);

builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([
        NodeApiName.Access,
        NodeApiName.Directories,
        NodeApiName.Organisations,
        NodeApiName.Search,
    ], tokenCredential);

builder.Services
    .Configure<BlockedEmailAddressOptions>(options => {
        var section = builder.Configuration.GetSection("BlockedEmailAddresses");
        options.BlockedDomains = section.GetJsonList("BlockedDomains");
        options.BlockedNames = section.GetJsonList("BlockedNames");
    })
    .AddInteractor<CheckIsBlockedEmailAddressUseCase>()
    .AddInteractor<AutoLinkEntraUserToDsiUseCase>();

await builder.Build().RunAsync();
