using System.Net;
using System.Reflection;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApi.Client.HttpSecurityProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// Extensions for adding Node.js API client services to a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Assembly ThisAssembly = typeof(ServiceCollectionExtensions).Assembly;

    /// <summary>
    /// Determines whether an interactor type descriptor is associated with a named
    /// DfE Sign-in mit-tier API (Node.js implementation).
    /// </summary>
    /// <param name="descriptor">The interactor type descriptor.</param>
    /// <param name="apiName">The name of a DfE Sign-in mid-tier API.</param>
    /// <returns>
    ///   <para>True when interactor is associated with the named mid-tier API;
    ///   otherwise, a value of false.</para>
    /// </returns>
    public static bool IsFor(this InteractorTypeDescriptor descriptor, NodeApiName apiName)
    {
        var attr = descriptor.ConcreteType.GetCustomAttribute<NodeApiAttribute>(inherit: true);
        return attr?.Name == apiName;
    }

    /// <summary>
    /// Adds services to support the Node.js API clients.
    /// </summary>
    /// <remarks>
    ///   <para>Ensure that service collection is configured with <see cref="NodeApiClientOptions"/>.</para>
    /// </remarks>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="apiNames">Indicates which mid-tier APIs are required.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="apiNames"/> is null.</para>
    /// </exception>
    public static void SetupNodeApiClient(
        this IServiceCollection services,
        IEnumerable<NodeApiName> apiNames)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(apiNames, nameof(apiNames));

        foreach (var apiName in apiNames) {
            services.AddHttpClient(apiName.ToString(), (provider, client) => {
                var apiOptions = GetNodeApiOptions(provider, apiName);
                client.BaseAddress = apiOptions.BaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler((provider) => {
                var apiOptions = GetNodeApiOptions(provider, apiName);
                return new HttpClientHandler {
                    UseProxy = apiOptions.AuthenticatedHttpClientOptions.UseProxy,
                    Proxy = new WebProxy(apiOptions.AuthenticatedHttpClientOptions.ProxyUrl)
                };
            })
            .AddHttpMessageHandler((provider) => {
                var apiOptions = GetNodeApiOptions(provider, apiName);
                string[] scopes = [$"{apiOptions.AuthenticatedHttpClientOptions.Resource}/.default"];
                var msal = provider.GetRequiredKeyedService<IConfidentialClientApplication>(apiName);
                var msalHttpSecurityProvider = new MsalHttpSecurityProvider(scopes, msal);
                return new AuthenticatedHttpClientHandler(msalHttpSecurityProvider);
            });

            // endpoints can use the httpClient via
            // app.MapGet("your-route", [FromKeyedServices(NodeApiName.Applications)] HttpClient client)
            services.AddKeyedSingleton(apiName, (provider, key) => {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient(key!.ToString()!);
                return client!;
            });

            services.AddKeyedSingleton(apiName, (provider, key) => {
                var apiOptions = GetNodeApiOptions(provider, apiName);
                return ConfidentialClientApplicationBuilder
                    .Create(apiOptions.AuthenticatedHttpClientOptions.ClientId.ToString())
                    .WithClientSecret(apiOptions.AuthenticatedHttpClientOptions.ClientSecret)
                    .WithAuthority(apiOptions.AuthenticatedHttpClientOptions.HostUrl)
                    .Build();
            });

            services.AddInteractors(
                InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(ThisAssembly)
                    .Where(descriptor => descriptor.IsFor(apiName))
            );
        }
    }

    private static NodeApiOptions GetNodeApiOptions(IServiceProvider provider, NodeApiName apiName)
    {
        return provider.GetRequiredService<IOptions<NodeApiClientOptions>>()
            .Value.Apis[apiName.ToString()];
    }
}
