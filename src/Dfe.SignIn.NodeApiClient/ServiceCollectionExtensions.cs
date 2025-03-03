using System.Reflection;
using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="descriptors">An enumerable collection of interactor type descriptors.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="apiNames"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="descriptors"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddNodeApiClient(this IServiceCollection services, IEnumerable<NodeApiName> apiNames, Action<NodeApiClientOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(apiNames, nameof(apiNames));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();
        services.Configure(setupAction);

        foreach (var apiName in apiNames) {
            services.AddInteractors(
                InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(ThisAssembly)
                    .Where(descriptor => descriptor.IsFor(apiName))
            );
        }

        return services;
    }
}
