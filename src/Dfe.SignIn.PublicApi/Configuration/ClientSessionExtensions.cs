using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Authorization;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up scoped sessions.
/// </summary>
public static class ClientSessionExtensions
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

        services.AddScoped<ClientSessionProvider>();
        services.AddScoped<IClientSession>(sp => sp.GetRequiredService<ClientSessionProvider>());
        services.AddScoped<IClientSessionWriter>(sp => sp.GetRequiredService<ClientSessionProvider>());
    }
}
