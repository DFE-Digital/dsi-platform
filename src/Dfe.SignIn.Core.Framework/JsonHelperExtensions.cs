using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework;

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
    /// Create standard JSON serialization options for the DfE Sign-in platform.
    /// </summary>
    /// <returns>
    ///   <para>The JSON serialization options.</para>
    /// </returns>
    public static JsonSerializerOptions CreateStandardOptions()
    {
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        };

        options.Converters.Add(new ExceptionJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }

    /// <summary>
    /// Setup JSON serialization options for the DfE Sign-in platform.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupDfeSignInJsonSerializerOptions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddKeyedSingleton(StandardOptionsKey, CreateStandardOptions());
    }
}
