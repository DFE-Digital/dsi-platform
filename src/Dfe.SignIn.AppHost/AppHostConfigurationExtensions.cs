using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Dfe.SignIn.AppHost;

internal static class AppHostConfigurationExtensions
{
    public static ConfigurationManager AddAppHostConfiguration(this ConfigurationManager configuration)
    {
        var assembly = typeof(AppHostConfigurationExtensions).Assembly;
        configuration.AddUserSecrets(assembly, optional: true, reloadOnChange: true);

        var connectionString = configuration.GetConnectionString("AppConfiguration");
        if (!string.IsNullOrEmpty(connectionString)) {
            var tag = configuration["AppConfiguration:Tag"];
            if (string.IsNullOrEmpty(tag)) {
                throw new InvalidOperationException("AppConfiguration Tag missing from configuration.");
            }

            configuration.AddAzureAppConfiguration(options => {
                options.Connect(connectionString)
                       .Select(KeyFilter.Any, tag)
                       .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
            });

            configuration.AddUserSecrets(assembly, optional: true, reloadOnChange: true);
        }

        return configuration;
    }
}
