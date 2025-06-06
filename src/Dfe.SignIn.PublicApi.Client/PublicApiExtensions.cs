using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Internal;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Users;
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

        services.ConfigureDfeSignInJsonSerializerOptions();
        services.ConfigureExternalModelJsonSerialization();

        SetupHttpClient(services);

        if (!services.Any(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(TimeProvider) ||
            typeof(TimeProvider).IsAssignableFrom(serviceDescriptor.ServiceType))) {
            services.AddSingleton(TimeProvider.System);
        }

        DiscoverCustomApiRequesters(services);
        AddSelectOrganisationApiRequesters(services);
        AddUsersApiRequesters(services);

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
        AddApiRequester(services, "v2/select-organisation", (client, jsonOptions, endpoint) =>
            new PublicApiPostRequester<
                CreateSelectOrganisationSession_PublicApiRequest,
                CreateSelectOrganisationSession_PublicApiResponse
            >(client, jsonOptions, endpoint)
        );

        // The following interactor is not exposed for use in applications until the
        // request/response models have been properly designed.
        services.AddInteractor<GetUserAccessToService_PublicApiRequester>();
    }

    private static void AddUsersApiRequesters(IServiceCollection services)
    {
        AddApiRequester(services, "v2/users/{userId}/organisations/{organisationId}/query", (client, jsonOptions, endpoint) =>
            new QueryUserOrganisation_PublicApiRequester(client, jsonOptions, endpoint)
        );
    }

    private static void AddApiRequester<TRequest, TResponse>(
        IServiceCollection services,
        string endpoint,
        Func<IPublicApiClient, JsonSerializerOptions, string, IInteractor<TRequest, TResponse>> factoryMethod)
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonOptionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
            var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
            return factoryMethod(client, jsonOptions, endpoint);
        });
    }
}
