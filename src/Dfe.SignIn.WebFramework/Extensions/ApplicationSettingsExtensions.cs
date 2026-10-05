namespace Dfe.SignIn.WebFramework.Extensions;

/// <summary>
/// Defines a contract for application settings that can be configured via the application's configuration system.
/// </summary>
public interface IApplicationSettings
{
    /// <summary>
    /// Gets the name of the configuration section for this option type.
    /// </summary>
    static abstract string SectionName { get; }
}

/// <summary>
/// Extension methods for Option configuration
/// </summary>
public static class ApplicationSettingsExtensions
{
    /// <summary>
    /// Registers application configuration with the service collection container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services">The service collection container</param>
    /// <param name="sectionName">The key relating to the configuration option to validate</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddOptionsWithValidation<T>(this IServiceCollection services, string? sectionName = null)
        where T : class, IApplicationSettings
    {
        sectionName ??= T.SectionName;

        services.AddOptions<T>()
                .BindConfiguration(sectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }
}
