using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Common helper functionality for JSON serialization.
/// </summary>
public static class JsonHelperExtensions
{
    /// <summary>
    /// Gets the unique options key for dependency injection.
    /// </summary>
    public const string StandardOptionsKey = "41007ea6-80ff-45fb-a42c-17fe3bad85a5";

    /// <summary>
    /// Creates a new instance of the standard <see cref="JsonSerializerOptions"/> to
    /// assist with unit testing.
    /// </summary>
    /// <returns>
    ///   <para>The standard <see cref="JsonSerializerOptions"/>.</para>
    /// </returns>
    public static JsonSerializerOptions CreateStandardOptionsTestHelper()
    {
        var services = new ServiceCollection();

        services.ConfigureDfeSignInJsonSerializerOptions();

        var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
        return accessor.Get(StandardOptionsKey);
    }

    /// <summary>
    /// Setup JSON serialization options for the DfE Sign-in platform.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The service collection.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection ConfigureDfeSignInJsonSerializerOptions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.Configure<JsonSerializerOptions>(StandardOptionsKey, options => {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;

            options.Converters.Add(new ExceptionJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        return services;
    }
}
