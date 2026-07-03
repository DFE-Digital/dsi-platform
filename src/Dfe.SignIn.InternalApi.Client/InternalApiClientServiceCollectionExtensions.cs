using Azure.Core;
using Dfe.SignIn.Core.Contracts.Features.Applications;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Extensions for adding internal API client services to a service collection.
/// </summary>
public static class InternalApiClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds a Refit client for the internal API users endpoint.
    /// </summary>
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
    public static IServiceCollection AddUsersApiClient(this IServiceCollection services, TokenCredential credential)
        => AddInternalApiClient<IUsersApiClient>(services, credential);

    /// <summary>
    /// Adds a Refit client for the internal API applications endpoint.
    /// </summary>
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
    public static IServiceCollection AddApplicationsApiClient(this IServiceCollection services, TokenCredential credential)
        => AddInternalApiClient<IApplicationsApiClient>(services, credential);

    private static IServiceCollection AddInternalApiClient<TClient>(
        IServiceCollection services,
        TokenCredential credential)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(credential);

        services
            .AddRefitClient<TClient>()
            .ConfigureHttpClient((provider, client) => {
                var options = provider.GetRequiredService<IOptions<InternalApiClientOptions>>().Value;
                client.BaseAddress = options.BaseAddress;
            })
            .AddHttpMessageHandler(provider => {
                var options = provider.GetRequiredService<IOptions<InternalApiClientOptions>>().Value;
                var scopes = new[] { $"{options.Resource}/.default" };
                var auditContextBuilder = provider.GetRequiredService<IAuditContextBuilder>();
                return new AuthenticatedHttpClientHandler(auditContextBuilder, credential, scopes);
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
