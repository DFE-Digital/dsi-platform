using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Audit;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Extension methods for setting up auditing features.
/// </summary>
public static class AuditConfigurationExtensions
{
    /// <summary>
    /// Setup auditing context for frontend and API applications.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupAuditContext(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuditContextBuilder, HttpAuditContextBuilder>();
    }
}
