using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Public;

/// <summary>
/// Extension methods to setup JSON serialization for external model types.
/// </summary>
[SuppressMessage("csharpsquid", "S125",
    Justification = "Commented out code provides an example of registering JSON converters."
)]
public static class PublicJsonExtensions
{
    /// <summary>
    /// Setup JSON serialiation for external models.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The service collection.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection ConfigureExternalModelJsonSerialization(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        // ConfigureExampleJsonSerialization(services);

        return services;
    }

    // private static void ConfigureExampleJsonSerialization(IServiceCollection services)
    // {
    //     services.Configure<JsonSerializerOptions>(JsonHelperExtensions.StandardOptionsKey, options => {
    //         options.Converters.Add(new ExampleJsonConverter());
    //     });
    // }
}
