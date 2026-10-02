namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for Option configuration
/// </summary>
public static class ApplicationSettingsHelpers
{
    /// <summary>
    /// Registers application configuration with the service collection container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services">The service collection container</param>
    /// <param name="sectionName">The key relating to the configuration option to validate</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddOptionsWithValidation<T>(this IServiceCollection services, string sectionName) where T : class
    {
        services.AddOptions<T>()
                .BindConfiguration(sectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }
}
