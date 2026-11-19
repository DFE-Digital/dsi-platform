using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dotnet;
using Aspire.Hosting.JavaScript;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost;

/// <summary>
/// Provides shared frontend asset configuration for .NET and Node.js resources.
/// </summary>
public static class FrontendResourceBuilderExtensions
{
    /// <summary>
    /// Configures a .NET resource with frontend assets and its frontend version.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="frontendAssets">The local frontend asset resource.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<DotnetProjectResource> WithFrontendAssets(
        this IResourceBuilder<DotnetProjectResource> builder,
        IResourceBuilder<ContainerResource> frontendAssets,
        IConfiguration configuration)
    {
        return ConfigureFrontendAssets(builder, frontendAssets, configuration, "Assets__BaseAddress")
            .WithEnvironment("Assets__FrontendVersion", configuration["Assets:FrontendVersion"]);
    }

    /// <summary>
    /// Configures a Node.js resource with frontend assets and CDN settings.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="frontendAssets">The local frontend asset resource.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<JavaScriptAppResource> WithFrontendAssets(
        this IResourceBuilder<JavaScriptAppResource> builder,
        IResourceBuilder<ContainerResource> frontendAssets,
        IConfiguration configuration)
    {
        if (configuration.GetSection("Components:Frontend").GetValue<bool>("UseHostedAssets")
            && string.IsNullOrWhiteSpace(configuration["Assets:BaseAddress"])) {
            throw new InvalidOperationException("Assets:BaseAddress is required when hosted frontend assets are enabled.");
        }

        return ConfigureFrontendAssets(builder, frontendAssets, configuration,
            "Assets__BaseAddress", "CDN_BASE_ADDRESS", "CDN_HOST_NAME");
    }

    private static IResourceBuilder<T> ConfigureFrontendAssets<T>(
        IResourceBuilder<T> builder,
        IResourceBuilder<ContainerResource> frontendAssets,
        IConfiguration configuration,
        params string[] environmentVariableNames)
        where T : IResourceWithEnvironment, IResourceWithWaitSupport
    {
        var useHostedAssets = configuration.GetSection("Components:Frontend").GetValue<bool>("UseHostedAssets");

        if (useHostedAssets) {
            var baseAddress = configuration["Assets:BaseAddress"];
            foreach (var name in environmentVariableNames) {
                builder = builder.WithEnvironment(name, baseAddress);
            }
        }
        else {
            var endpoint = frontendAssets.GetEndpoint("http");
            foreach (var name in environmentVariableNames) {
                builder = builder.WithEnvironment(name, endpoint);
            }
        }

        return builder.WaitFor(frontendAssets);
    }
}
