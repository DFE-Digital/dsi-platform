using System.Net;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Extensions for adding internal API client services to a service collection.
/// </summary>
public static class InternalApiServiceCollectionExtensions
{
    /// <summary>
    /// The unique service key for the internal API <see cref="HttpClient"/>.
    /// </summary>
    public const string InternalApiKey = "InternalApi";

    /// <summary>
    /// Adds services to support the internal API client.
    /// </summary>
    /// <remarks>
    ///   <para>Ensure that service collection is configured with <see cref="InternalApiClientOptions"/>.</para>
    /// </remarks>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="credential">Credential providing tokens for Node API requests.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="credential"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupInternalApiClient(
        this IServiceCollection services,
        TokenCredential credential)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(credential, nameof(credential));

        services
            .AddHttpClient(InternalApiKey, (provider, client) => {
                var apiOptions = provider.GetRequiredService<IOptions<InternalApiClientOptions>>().Value;
                client.BaseAddress = apiOptions.BaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler((provider) => {
                var apiOptions = provider.GetRequiredService<IOptions<InternalApiClientOptions>>().Value;
                return new HttpClientHandler {
                    UseProxy = apiOptions.UseProxy,
                    Proxy = new WebProxy(apiOptions.ProxyUrl)
                };
            })
            .AddHttpMessageHandler((provider) => {
                var apiOptions = provider.GetRequiredService<IOptions<InternalApiClientOptions>>().Value;
                string[] scopes = [$"{apiOptions.Resource}/.default"];
                return ActivatorUtilities.CreateInstance<AuthenticatedHttpClientHandler>(provider, credential, scopes);
            });

        services.AddKeyedSingleton(InternalApiKey, (provider, key) => {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(key!.ToString()!);
            return client!;
        });

        services.Decorate<IInteractorResolver, InternalApiInteractorResolver>();

        return services;
    }
}
