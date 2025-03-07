using System.Net;
using System.Reflection;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApiClient.HttpSecurityProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace Dfe.SignIn.NodeApiClient;

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
    // </returns>
    public static bool IsFor(this InteractorTypeDescriptor descriptor, NodeApiName apiName)
    {
        var attr = descriptor.ConcreteType.GetCustomAttribute<NodeApiAttribute>(inherit: true);
        return attr?.Name == apiName;
    }

    /// <summary>
    /// Adds services to support the Node.js API clients.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="apiNames">Indicates which mid-tier APIs are required.</param>
    /// <param name="setupAction">An action to configure the provided options.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="setupAction"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddNodeApiClient(
        this IServiceCollection services,
        Action<NodeApiClientOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();
        services.Configure(setupAction);

        var instanceOptions = new NodeApiClientOptions();
        setupAction(instanceOptions);

        foreach (var api in instanceOptions.Apis) {

            services.AddHttpClient(api.ApiName.ToString(), client => {
                client.BaseAddress = api.BaseAddress;
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                UseProxy = api.AuthenticatedHttpClientOptions.UseProxy,
                Proxy = new WebProxy(api.AuthenticatedHttpClientOptions.ProxyUrl)
            })
            .AddHttpMessageHandler(provider => {
                string[] scopes = [$"{api.AuthenticatedHttpClientOptions.Resource}/.default"];
                var msal = provider.GetRequiredKeyedService<IConfidentialClientApplication>(api.ApiName);
                var msalHttpSecurityProvider = new MsalHttpSecurityProvider(scopes, msal);
                return new AuthenticatedHttpClientHandler(msalHttpSecurityProvider);
            });

            // endpoints can use the httpClient via  
            // app.MapGet("your-route", [FromKeyedServices(NodeApiName.Applications)] HttpClient client)
            services.AddKeyedSingleton(api.ApiName, (provider, key) => {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient(key!.ToString()!);
                return client!;
            });

            services.AddKeyedSingleton(api.ApiName, (provider, key) => {
                return ConfidentialClientApplicationBuilder
                    .Create(api.AuthenticatedHttpClientOptions.ClientId.ToString())
                    .WithClientSecret(api.AuthenticatedHttpClientOptions.ClientSecret)
                    .WithAuthority(api.AuthenticatedHttpClientOptions.HostUrl)
                    .Build();
            });

            services.AddInteractors(
                InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(ThisAssembly)
                    .Where(descriptor => descriptor.IsFor(api.ApiName))
            );
        }

        return services;
    }
}
