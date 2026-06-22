using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.WebFramework.Extensions;

/// <summary>
/// Options extensions used to add configuration and apply attribute validation
/// </summary>
[ExcludeFromCodeCoverage]
public static class OptionsExtensions
{
    /// <summary>
    /// Adds options and performs attribute based validation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddValidatedRequiredOptions<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where T : class
    {
        services.AddOptions<T>()
            .Bind(configuration.GetRequiredSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
