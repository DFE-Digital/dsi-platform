using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.Client.Internal;
using Dfe.SignIn.PublicApi.Client.Users;
using Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;
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

        services.AddInteractionFramework();
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
                CreateSelectOrganisationSessionApiRequest,
                CreateSelectOrganisationSessionApiResponse
            >(client, jsonOptions, endpoint)
        );

        // The following interactor is not exposed for use in applications until the
        // request/response models have been properly designed.
        services.AddInteractor<GetUserAccessToServiceApiRequester>();
    }

    private static void AddUsersApiRequesters(IServiceCollection services)
    {
        AddApiRequester(services, "v2/users/{userId}/organisations/{organisationId}/query", (client, jsonOptions, endpoint) =>
            new QueryUserOrganisationApiRequester(client, jsonOptions, endpoint)
        );
    }

    private static void AddApiRequester<TRequest>(
        IServiceCollection services,
        string endpoint,
        Func<IPublicApiClient, JsonSerializerOptions, string, IInteractor<TRequest>> factoryMethod)
        where TRequest : class
    {
        services.AddTransient(provider => {
            var client = provider.GetRequiredService<IPublicApiClient>();
            var jsonOptionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
            var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
            return factoryMethod(client, jsonOptions, endpoint);
        });
    }
}
