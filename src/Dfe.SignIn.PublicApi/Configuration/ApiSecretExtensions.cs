
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.UseCases.PublicApi;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Provides extension methods for configuring ApiSecret encryption/decryption
/// services, such that PublicApi can handle encrypted ApiSecrets.
/// </summary>
public static class ApiSecretExtensions
{
    /// <summary>
    /// Configures the services required to encrypt and decrypt ApiSecrets.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    public static void SetupApiSecretEncryption(this IServiceCollection services, IConfiguration configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        services.Configure<ApiSecretEncryptionOptions>(configuration.GetRequiredSection("PublicApiSecretEncryption"));
        services.AddInteractor<DecryptApiSecretUseCase>();
    }
}
