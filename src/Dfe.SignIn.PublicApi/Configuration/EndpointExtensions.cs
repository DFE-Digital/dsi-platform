using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <exclude/>
public static class EndpointExtensions
{
    /// <summary>
    /// General setup of endpoints.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupEndpoints(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        SetupCamelCaseNamingForEnums(services);
    }

    private static void SetupCamelCaseNamingForEnums(IServiceCollection services)
    {
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293
        services.ConfigureHttpJsonOptions(options => {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
                namingPolicy: JsonNamingPolicy.CamelCase
            ));
        });
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(
                namingPolicy: JsonNamingPolicy.CamelCase
            ));
        });
    }
}
