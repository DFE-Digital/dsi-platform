using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.ScopedSession;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up scoped sessions.
/// </summary>
public static class ScopedSessionExtensions
{
    /// <summary>
    /// Setup ScopedSession and specify any custom mapping profiles.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupScopedSession(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddScoped<IScopedSessionReader, ScopedSessionProvider>();
        services.AddScoped<IScopedSessionWriter>(sp => (ScopedSessionProvider)sp.GetRequiredService<IScopedSessionReader>());
    }
}
