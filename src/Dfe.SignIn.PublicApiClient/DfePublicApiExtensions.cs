using Dfe.SignIn.PublicApiClient.PublicApiSigning;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApiClient;

/// <summary>
/// Extension methods for setting up a client to the DfE Sign-in Public API.
/// </summary>
public static class DfePublicApiExtensions
{
    /// <summary>
    /// Setup client to the DfE Sign-in Public API.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupDfePublicApiClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddOptions();
        services.Configure<PublicKeyCacheOptions>(_ => { });

        // TODO: Setup the HttpClient properly...
        services.AddKeyedSingleton<HttpClient>(DfePublicApiConstants.HttpClientKey);

        services.AddSingleton<IPublicKeyCache, PublicKeyCache>();
        services.AddSingleton<IPayloadVerifier, DefaultPayloadVerifier>();

        return services;
    }
}
