using Azure.Identity;
using Dfe.SignIn.Base.Framework;
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

// Get token credential for making API requests to Node APIs.
var apiConfigurationSection = builder.Configuration.GetRequiredSection("NodeApiClient:Apis:Access:AuthenticatedHttpClientOptions");
var tokenCredential = new ClientSecretCredential(
    tenantId: apiConfigurationSection.GetValue<string>("Tenant"),
    clientId: apiConfigurationSection.GetValue<string>("ClientId"),
    clientSecret: apiConfigurationSection.GetValue<string>("ClientSecret"),
    new TokenCredentialOptions {
        AuthorityHost = apiConfigurationSection.GetValue<Uri>("HostUrl"),
    }
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
