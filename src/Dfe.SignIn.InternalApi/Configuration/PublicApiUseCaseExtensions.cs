using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.PublicApi;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for setting up "Public API" use cases.
/// </summary>
public static class PublicApiUseCaseExtensions
{
    /// <summary>
    /// Adds use case interactors.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddPublicApiUseCases(
        this IServiceCollection services, IConfigurationRoot configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.Configure<ApiSecretEncryptionOptions>(configuration.GetRequiredSection("PublicApiSecretEncryption"));
        services.AddInteractor<EncryptApiSecretUseCase>();
        services.AddInteractor<DecryptApiSecretUseCase>();

        return services;
    }
}
