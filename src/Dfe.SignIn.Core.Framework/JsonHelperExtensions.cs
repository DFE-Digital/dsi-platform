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
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddKeyedSingleton(StandardOptionsKey, CreateStandardOptions());
        services.AddSingleton<IJsonSerializerOptionsAccessor, JsonSerializerOptionsAccessor>();
    }
}

/// <summary>
/// Represents a service that gets the <see cref="JsonHelperExtensions.StandardOptionsKey"/>
/// <see cref="JsonSerializerOptions"/> instance.
/// </summary>
/// <remarks>
///   <para>This was added to workaround a bug in .NET Core: https://github.com/dotnet/aspnetcore/issues/54500</para>
/// </remarks>
public interface IJsonSerializerOptionsAccessor
{
    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> instanced.
    /// </summary>
    /// <returns>
    ///   <para>The <see cref="JsonSerializerOptions"/> instance.
    /// </returns>
    JsonSerializerOptions GetOptions();
}

internal sealed class JsonSerializerOptionsAccessor(
    [FromKeyedServices(JsonHelperExtensions.StandardOptionsKey)] JsonSerializerOptions jsonOptions
) : IJsonSerializerOptionsAccessor
{
    /// <inheritdoc />
    public JsonSerializerOptions GetOptions() => jsonOptions;
}
