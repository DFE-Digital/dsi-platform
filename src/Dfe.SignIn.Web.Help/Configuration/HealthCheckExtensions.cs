using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Web.Help.Configuration;

/// <summary>
/// Extension methods for setting up health checks.
/// </summary>
[ExcludeFromCodeCoverage]
public static class HealthCheckExtensions
{
    /// <summary>
    /// Setup and register the heathcheck service.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupHealthChecks(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddHealthChecks();
    }
}
