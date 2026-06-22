using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Utility method for loading settings from Azure App Configuration
/// </summary>
[ExcludeFromCodeCoverage]
public static class AzureAppConfigHelper
{
    /// <summary>
    /// Sets up application to read settings from Azure App Configuration
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="FilterNames">The desired tags to load</param>
    public static void LoadSettingsFromAzureAppConfig(this WebApplicationBuilder builder, string[] FilterNames)
    {
        builder.Services.AddOptions<AzureAppConfig>()
        .Bind(builder.Configuration.GetSection("AzureAppConfiguration"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

        var configOptions = builder.Configuration
            .GetSection("AzureAppConfiguration")
            .Get<AzureAppConfig>()!;

        builder.Configuration.AddAzureAppConfiguration(opts => {
            opts.Connect(configOptions.Endpoint);

            foreach (var label in FilterNames) {
                opts.Select(KeyFilter.Any, label);
            }

            opts.ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
            opts.ConfigureRefresh(refreshOptions => {
                refreshOptions.Register("Sentinel", refreshAll: true)
                              .SetRefreshInterval(TimeSpan.FromSeconds(60));
            });
        });
    }
}
