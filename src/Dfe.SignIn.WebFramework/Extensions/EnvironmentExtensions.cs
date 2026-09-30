using Microsoft.AspNetCore.Hosting;

namespace Dfe.SignIn.WebFramework.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IWebHostEnvironment"/> interface.
/// </summary>
public static class EnvironmentExtensions
{
    /// <summary>
    /// Determines whether the current environment is Local.
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    public static bool IsLocalEnvironment(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment(EnvironmentName.Local);
    }

    /// <summary>
    /// Gets the <see cref="EnvironmentName"/> for the current environment.
    /// </summary>
    /// <param name="environment">The web host environment.</param>
    /// <param name="expectedEnvironment"></param>
    /// <returns>The corresponding <see cref="EnvironmentName"/>.</returns>
    private static bool IsEnvironment(this IWebHostEnvironment environment, EnvironmentName expectedEnvironment)
    {
        var currentEnvironmentName = GetEnvironmentName(expectedEnvironment);
        return environment.EnvironmentName == currentEnvironmentName;
    }

    /// <summary>
    /// Gets the string representation of the specified <see cref="EnvironmentName"/>.
    /// </summary>
    /// <param name="environment">The environment name.</param>
    /// <returns>The string representation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetEnvironmentName(EnvironmentName environment)
    {
        return environment switch {
            EnvironmentName.Local => "Local",
            EnvironmentName.Dev => "Dev",
            EnvironmentName.Test => "Test",
            EnvironmentName.Tran => "Tran",
            EnvironmentName.PreProd => "PreProd",
            EnvironmentName.Prod => "Prod",
            _ => throw new InvalidOperationException($"Unknown environment name: {environment}")
        };
    }
}
