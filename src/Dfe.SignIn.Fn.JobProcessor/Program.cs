using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.InternalApi.Client;
using Dfe.SignIn.NodeApi.Client;
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

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddInteractionFramework();

// Get token credential for making API requests to internal APIs.
var tokenCredential = TokenCredentialHelpers.CreateFromConfiguration(
    builder.Configuration.GetRequiredSection("InternalApiClient")
);

builder.Services
    .Configure<InternalApiClientOptions>(builder.Configuration.GetRequiredSection("InternalApiClient"))
    .SetupInternalApiClient(tokenCredential)
    .SetupResiliencePipelines(builder.Configuration);

// builder.Services
//     .AddInteractor<ExampleOfJobsSpecificUseCase>()
//     .AddInteractor<AnotherExampleOfJobsSpecificUseCase>();

await builder.Build().RunAsync();
