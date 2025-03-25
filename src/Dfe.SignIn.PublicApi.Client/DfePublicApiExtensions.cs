using System.Text.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
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

        services.SetupDfeSignInJsonSerializerOptions();

        SetupHttpClient(services);

        services.AddSingleton<IPublicKeyCache, PublicKeyCache>();
        services.AddSingleton<IPayloadVerifier, DefaultPayloadVerifier>();

        DiscoverCustomApiRequesters(services);
        AddSelectOrganisationApiRequesters(services);

        return services;
    }

    private static void SetupHttpClient(IServiceCollection services)
    {
        services.AddTransient<PublicApiBearerTokenHandler>();
        services.AddHttpClient("DsiPublicApi", ConfigureHttpClient)
            .AddHttpMessageHandler<PublicApiBearerTokenHandler>();

        services.AddKeyedSingleton(DfePublicApiConstants.HttpClientKey, (provider, key) => {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient("DsiPublicApi");
        });

        services.AddSingleton<IPublicApiClient, PublicApiClient>();
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

    private static void DiscoverCustomApiRequesters(IServiceCollection services)
    {
        services.AddInteractors(
            InteractorReflectionHelpers.DiscoverAnnotatedInteractorsInAssembly<PublicApiRequesterAttribute>(
                typeof(DfePublicApiExtensions).Assembly)
        );
    }

    private static void AddSelectOrganisationApiRequesters(IServiceCollection services)
    {
        AddPostRequester<
            CreateSelectOrganisationSession_PublicApiRequest,
            CreateSelectOrganisationSession_PublicApiResponse
        >(services, "v2/select-organisation");
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static void AddGetRequester<TRequest, TResponse>(IServiceCollection services, string endpoint)
#pragma warning restore IDE0051 // Remove unused private members
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient<IInteractor<TRequest, TResponse>>(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonSerializerOptions = provider.GetRequiredKeyedService<JsonSerializerOptions>(JsonHelperExtensions.StandardOptionsKey);
            return new PublicApiGetRequester<TRequest, TResponse>(client, jsonSerializerOptions, endpoint);
        });
    }

    private static void AddPostRequester<TRequest, TResponse>(IServiceCollection services, string endpoint)
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient<IInteractor<TRequest, TResponse>>(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonSerializerOptions = provider.GetRequiredKeyedService<JsonSerializerOptions>(JsonHelperExtensions.StandardOptionsKey);
            return new PublicApiPostRequester<TRequest, TResponse>(client, jsonSerializerOptions, endpoint);
        });
    }
}
