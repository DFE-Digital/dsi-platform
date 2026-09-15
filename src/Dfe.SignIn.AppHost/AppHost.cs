using Dfe.SignIn.AppHost;
using Dfe.SignIn.AppHost.Resources;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDsiConfiguration();

var components = builder.Configuration
    .GetSection(ComponentSettings.SectionName)
    .Get<ComponentSettings>() ?? new ComponentSettings();

var infra = builder.AddPlatformInfrastructure();
var internalApi = builder.AddInternalApi(infra);

if (components.DotNet.Help) {
    builder.AddHelpApp(infra, internalApi);
}

if (components.DotNet.Profile) {
    builder.AddProfileApp(infra, internalApi);
}

if (components.DotNet.PublicApi) {
    builder.AddPublicApi(infra, internalApi);
}

builder.AddNodePlatformApps(infra, components.Node);

if (components.Tools.TlsProxy) {
    builder.AddTlsProxy();
}

await builder.Build().RunAsync();
