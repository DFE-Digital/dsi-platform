
using Dfe.SignIn.PublicApi.ScopedSession;

/// <exclude/>
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
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddScoped<IScopedSessionReader, ScopedSessionProvider>();
        services.AddScoped<IScopedSessionWriter>(sp => (ScopedSessionProvider)sp.GetRequiredService<IScopedSessionReader>());
    }
}