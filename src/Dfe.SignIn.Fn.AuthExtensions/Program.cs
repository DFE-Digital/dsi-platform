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

builder.Services
    .Configure<NodeApiClientOptions>(builder.Configuration.GetRequiredSection("NodeApiClient"))
    .SetupNodeApiClient([
        NodeApiName.Access,
        NodeApiName.Directories,
        NodeApiName.Organisations,
        NodeApiName.Search,
    ]);

builder.Services
    .Configure<BlockedEmailAddressOptions>(options => {
        var section = builder.Configuration.GetSection("BlockedEmailAddresses");
        options.BlockedDomains = section.GetJsonList("BlockedDomains");
        options.BlockedNames = section.GetJsonList("BlockedNames");
    })
    .AddInteractor<CheckIsBlockedEmailAddressUseCase>()
    .AddInteractor<AutoLinkEntraUserToDsiUseCase>();

builder.Build().Run();
