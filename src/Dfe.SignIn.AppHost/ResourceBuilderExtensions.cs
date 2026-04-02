using Microsoft.Extensions.Configuration;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dfe.SignIn.AppHost;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> WithServiceConfiguration(
        this IResourceBuilder<ProjectResource> builder,
        IConfiguration configuration,
        string serviceName)
    {
        // 1. Apply Shared Settings
        var sharedSettings = configuration.GetSection("Shared");
        foreach (var child in sharedSettings.AsEnumerable())
        {
            if (child.Value != null)
            {
                var envKey = child.Key.Replace("Shared:", "").Replace(":", "__");
                builder.WithEnvironment(envKey, child.Value);
            }
        }

        // 2. Apply Service-Specific Settings (overrides shared if same key)
        var serviceSettings = configuration.GetSection($"Services:{serviceName}");
        foreach (var child in serviceSettings.AsEnumerable())
        {
            if (child.Value != null)
            {
                var envKey = child.Key.Replace($"Services:{serviceName}:", "").Replace(":", "__");
                builder.WithEnvironment(envKey, child.Value);
            }
        }

        return builder;
    }
}
