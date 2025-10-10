using System.Net;
using System.Reflection;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// Extensions for adding Node.js API client services to a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Assembly ThisAssembly = typeof(ServiceCollectionExtensions).Assembly;

    /// <summary>
    /// Determines whether all of the middle-tier APIs that are required by an
    /// interactor type descriptor are available.
    /// </summary>
    /// <param name="descriptor">The interactor type descriptor.</param>
    /// <param name="availableApis">The names of available middle-tier APIs.</param>
    /// <returns>
    ///   <para>True when interactor is associated with the named mid-tier API;
    ///   otherwise, a value of false.</para>
    /// </returns>
    public static bool AreAllRequiredApisAvailable(this InteractorTypeDescriptor descriptor, IEnumerable<NodeApiName> availableApis)
    {
        var attrs = descriptor.ConcreteType.GetCustomAttributes<NodeApiAttribute>(inherit: true);
        var availableAttrs = attrs.IntersectBy(availableApis, attr => attr.Name);
        return attrs.Any() && availableAttrs.Count() == attrs.Count();
    }

    /// <summary>
    /// Adds services to support the Node.js API clients.
    /// </summary>
    /// <remarks>
    ///   <para>Ensure that service collection is configured with <see cref="NodeApiClientOptions"/>.</para>
    /// </remarks>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="apiNames">Indicates which mid-tier APIs are required.</param>
    /// <param name="credential">Credential providing tokens for Node API requests.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="apiNames"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="credential"/> is null.</para>
    /// </exception>
    public static IServiceCollection SetupNodeApiClient(
        this IServiceCollection services,
        IEnumerable<NodeApiName> apiNames,
        TokenCredential credential)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(apiNames, nameof(apiNames));
        ExceptionHelpers.ThrowIfArgumentNull(credential, nameof(credential));

        foreach (var apiName in apiNames) {
            services.AddHttpClient(apiName.ToString(), (provider, client) => {
                var apiOptions = GetNodeApiOptions(provider, apiName);
                client.BaseAddress = apiOptions.BaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler((provider) => {
                var options = GetNodeApiOptions(provider, apiName);
                return new HttpClientHandler {
                    UseProxy = options.AuthenticatedHttpClientOptions.UseProxy,
                    Proxy = new WebProxy(options.AuthenticatedHttpClientOptions.ProxyUrl)
                };
            })
            .AddHttpMessageHandler((provider) => {
                var options = GetNodeApiOptions(provider, apiName);
                string[] scopes = [$"{options.AuthenticatedHttpClientOptions.Resource}/.default"];
                return new AuthenticatedHttpClientHandler(credential, scopes);
            });

            // endpoints can use the httpClient via
            // app.MapGet("your-route", [FromKeyedServices(NodeApiName.Applications)] HttpClient client)
            services.AddKeyedSingleton(apiName, (provider, key) => {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient(key!.ToString()!);
                return client!;
            });
        }

        services.AddInteractors(
            InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(ThisAssembly)
                .Where(descriptor => descriptor.AreAllRequiredApisAvailable(apiNames))
        );

        return services;
    }

    private static NodeApiOptions GetNodeApiOptions(IServiceProvider provider, NodeApiName apiName)
    {
        return provider.GetRequiredService<IOptions<NodeApiClientOptions>>()
            .Value.Apis[apiName.ToString()];
    }
}
