using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client;

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

        services.AddSingleton<PublicApiBearerTokenHandler>();
        services.AddHttpClient(DfePublicApiConstants.HttpClientKey, ConfigureHttpClient)
            .AddHttpMessageHandler<PublicApiBearerTokenHandler>();

        services.AddSingleton<IPublicKeyCache, PublicKeyCache>();
        services.AddSingleton<IPayloadVerifier, DefaultPayloadVerifier>();

        return services;
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        var optionsAccessor = provider.GetRequiredService<IOptions<DfePublicApiOptions>>();
        var options = optionsAccessor.Value;

        if (options.BaseAddress is null) {
            throw new InvalidOperationException("Invalid DfE Sign-in Public API base address");
        }

        client.BaseAddress = options.BaseAddress;
    }
}
