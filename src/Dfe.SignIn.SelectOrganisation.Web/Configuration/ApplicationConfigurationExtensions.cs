namespace Dfe.SignIn.SelectOrganisation.Web.Configuration;

/// <summary>
/// Extension methods for setting up the application.
/// </summary>
public static class ApplicationConfigurationExtensions
{
    /// <summary>
    /// Setup application specific configuration.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="setupAction">An action to configure the provided options.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="setupAction"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddApplication(this IServiceCollection services, Action<ApplicationOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();

        services.Configure(setupAction);

        return services;
    }
}
