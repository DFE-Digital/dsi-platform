using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.InternalApi.Contracts;
using Dfe.SignIn.InternalApi.Swagger;
using Microsoft.OpenApi.Models;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Extension methods for setting up Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SwaggerExtensions
{
    private static string GetXmlFileName(Type type)
    {
        return type.Assembly.Location
            .Replace(".dll", ".xml")
            .Replace(".exe", ".xml");
    }

    /// <summary>
    /// Setup swagger for endpoints.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSwagger(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(config => {
            config.UseInlineDefinitionsForEnums();

            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                Description = "Enter your JWT bearer token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });

            // Group endpoints by the first segment of the route path
            config.TagActionsBy(api => [NamingHelpers.GetSchemaGroupFromPath(api.RelativePath)]);

            // Use nicer syntax for generics in schema IDs.
            config.CustomSchemaIds(NamingHelpers.NiceifySchemaId);

            // Enable example values from `SwaggerExampleValueAttribute` annotations.
            config.SchemaFilter<SwaggerExampleValueSchemaFilter>();

            // Include XML comments for 'SignIn.Core.Contracts.dll' assembly.
            config.IncludeXmlComments(GetXmlFileName(typeof(CoreContractsAssembly)));
            // Include XML comments for 'SignIn.Core.InternalApi.Contracts.dll' assembly.
            config.IncludeXmlComments(GetXmlFileName(typeof(InteractionResponse<>)));

            config.SwaggerDoc("v1", new OpenApiInfo {
                Title = "DfE Sign-in Internal API",
                Version = "v1"
            });
        });
    }
}
