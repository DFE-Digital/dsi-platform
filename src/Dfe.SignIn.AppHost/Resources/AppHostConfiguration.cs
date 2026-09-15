using System.Reflection;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Dfe.SignIn.AppHost.Resources;

/// <summary>
/// Configures AppHost configuration sources (user secrets, Azure App Configuration).
/// </summary>
public static class AppHostConfiguration
{
    /// <summary>
    /// Adds user secrets and, when configured, Azure App Configuration with Key Vault support.
    /// </summary>
    /// <remarks>
    /// User secrets are applied twice when Azure App Configuration is enabled: once first so
    /// connection details are available, then again afterwards so local secret overrides win.
    /// </remarks>
    public static IDistributedApplicationBuilder AddDsiConfiguration(
        this IDistributedApplicationBuilder builder)
    {
        builder.Configuration.AddUserSecrets(
            Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);

        var appConfigurationConnectionString = builder.Configuration.GetConnectionString("AppConfiguration");
        if (string.IsNullOrEmpty(appConfigurationConnectionString)) {
            return builder;
        }

        var appConfigurationTag = builder.Configuration["AppConfiguration:Tag"];
        if (string.IsNullOrEmpty(appConfigurationTag)) {
            throw new InvalidOperationException("AppConfiguration Tag missing from configuration.");
        }

        builder.Configuration.AddAzureAppConfiguration(options => {
            options.Connect(appConfigurationConnectionString)
                   .Select(KeyFilter.Any, appConfigurationTag)
                   .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
        });

        // Re-apply user secrets so local overrides win over Azure App Configuration.
        builder.Configuration.AddUserSecrets(
            Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);

        return builder;
    }
}
