using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Internal;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension methods for setting up a client to the DfE Sign-in Public API.
/// </summary>
public static class PublicApiExtensions
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
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddOptions();
        services.Configure<PublicKeyCacheOptions>(_ => { });

        services.ConfigureDfeSignInJsonSerializerOptions();
        services.ConfigureExternalModelJsonSerialization();

        SetupHttpClient(services);

        if (!services.Any(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(TimeProvider) ||
            typeof(TimeProvider).IsAssignableFrom(serviceDescriptor.ServiceType))) {
            services.AddSingleton(TimeProvider.System);
        }

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

        services.AddKeyedSingleton(PublicApiConstants.HttpClientKey, (provider, key) => {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient("DsiPublicApi");
        });

        services.AddSingleton<IPublicApiClient, PublicApiClient>();
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        var optionsAccessor = provider.GetRequiredService<IOptions<PublicApiOptions>>();
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
                typeof(PublicApiExtensions).Assembly)
        );
    }

    private static void AddSelectOrganisationApiRequesters(IServiceCollection services)
    {
        AddPostRequester<
            CreateSelectOrganisationSession_PublicApiRequest,
            CreateSelectOrganisationSession_PublicApiResponse
        >(services, "v2/select-organisation");

        // The following interactor is not exposed for use in applications until the
        // request/response models have been properly designed.
        services.AddInteractor<GetUserAccessToService_PublicApiRequester>();
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static void AddGetRequester<TRequest, TResponse>(IServiceCollection services, string endpoint)
#pragma warning restore IDE0051 // Remove unused private members
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient<IInteractor<TRequest, TResponse>>(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonOptionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
            var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
            return new PublicApiGetRequester<TRequest, TResponse>(client, jsonOptions, endpoint);
        });
    }

    private static void AddPostRequester<TRequest, TResponse>(IServiceCollection services, string endpoint)
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient<IInteractor<TRequest, TResponse>>(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonOptionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
            var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
            return new PublicApiPostRequester<TRequest, TResponse>(client, jsonOptions, endpoint);
        });
    }
}
