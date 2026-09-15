using Dfe.SignIn.AppHost;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.Resources;

/// <summary>
/// Registers Node.js platform application resources.
/// </summary>
public static class NodeAppResources
{
    /// <summary>
    /// Adds enabled Node platform apps and wires their dependency order
    /// (OIDC → Interactions → Services).
    /// </summary>
    public static void AddNodePlatformApps(
        this IDistributedApplicationBuilder builder,
        PlatformInfrastructure infra,
        NodeComponentSettings settings)
    {
        if (!settings.Oidc && !settings.Interactions && !settings.Services) {
            return;
        }

        var nodeRootDir = builder.Configuration["NodePlatformDirectory"]
            ?? throw new InvalidOperationException("NodePlatformDirectory is not configured.");

        var nodeEnvFileName = builder.Configuration["NodeEnvFileName"]
            ?? throw new InvalidOperationException("NodeEnvFileName is not configured.");

        IResourceBuilder<NodeAppResource>? oidc = null;
        if (settings.Oidc) {
            oidc = builder.AddNodePlatformApp(
                    "node-oidc",
                    nodeRootDir,
                    "login.dfe.oidc",
                    port: 4436,
                    infra.NodeRedisConnectionString,
                    infra.FrontendEndpoint,
                    envFileName: nodeEnvFileName,
                    scriptName: "dev")
                .WithNpmPackageInstallation()
                .WaitFor(infra.Redis);
        }

        IResourceBuilder<NodeAppResource>? interactor = null;
        if (settings.Interactions) {
            interactor = builder.AddNodePlatformApp(
                    "node-interactions",
                    nodeRootDir,
                    "login.dfe.interactions",
                    port: 4431,
                    infra.NodeRedisConnectionString,
                    infra.FrontendEndpoint,
                    envFileName: nodeEnvFileName,
                    scriptName: "dev")
                .WithNpmPackageInstallation()
                .WaitForIfPresent(oidc);
        }

        if (settings.Services) {
            builder.AddNodePlatformApp(
                    "node-services",
                    nodeRootDir,
                    "login.dfe.services",
                    port: 41012,
                    infra.NodeRedisConnectionString,
                    infra.FrontendEndpoint,
                    envFileName: nodeEnvFileName)
                .WithNpmPackageInstallation()
                .WaitForIfPresent(oidc)
                .WaitForIfPresent(interactor);
        }
    }
}
