using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dfe.SignIn.InternalApi.Swagger;

/// <summary>
/// Schema filter which generates information about serialized exceptions showing
/// only properties that have been annotated with <see cref="PersistAttribute"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SwaggerExceptionSchemaFilter : ISchemaFilter
{
    private static readonly Type[] OpenFailedInteractionResponseTypes = [
        typeof(FailedInteractionResponse<>),
        typeof(NotFoundInteractionResponse<>),
    ];

    /// <summary>
    /// Apply is called for each parameter
    /// </summary>
    /// <param name="schema">The OpenAPI schema.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || !OpenFailedInteractionResponseTypes.Contains(context.Type.GetGenericTypeDefinition())) {
            return;
        }

        bool isNotFoundResponse = context.Type.GetGenericTypeDefinition() == typeof(NotFoundInteractionResponse<>);
        var requestType = context.Type.GenericTypeArguments[0];

        var throwsAttributes = Attribute
            .GetCustomAttributes(requestType, typeof(ThrowsAttribute), inherit: true)
            .Cast<ThrowsAttribute>()
            .Where(x => (isNotFoundResponse && typeof(NotFoundInteractionException).IsAssignableFrom(x.ExceptionType))
                || (!isNotFoundResponse && !typeof(NotFoundInteractionException).IsAssignableFrom(x.ExceptionType)));

        var exceptionSchemas = throwsAttributes.Select(x => CreateSchemaForExceptionType(x.ExceptionType));
        if (!isNotFoundResponse) {
            exceptionSchemas = exceptionSchemas.Append(CreateSchemaForExceptionType(typeof(UnexpectedException)));
        }

        schema.Properties["exception"] = exceptionSchemas.Count() > 1
            ? new OpenApiSchema { AnyOf = [.. exceptionSchemas] }
            : exceptionSchemas.First();
    }

    private static OpenApiSchema CreateSchemaForExceptionType(Type exceptionType)
    {
        var exceptionSchema = new OpenApiSchema {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
        };

        exceptionSchema.Properties["type"] = new OpenApiSchema {
            Type = "string",
            Default = new OpenApiString(exceptionType.FullName),
        };

        exceptionSchema.Properties["message"] = new OpenApiSchema {
            Type = "string",
        };

        var persistedProperties = exceptionType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.HasAttribute<PersistAttribute>());

        foreach (var property in persistedProperties) {
            exceptionSchema.Properties[ToCamelCase(property.Name)] = new OpenApiSchema {
                Type = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(property.PropertyType).Type,
                Nullable = property.PropertyType.IsReferenceOrNullableType(),
            };
        }

        return exceptionSchema;
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 2) {
            return input;
        }
        return char.ToLowerInvariant(input[0]) + input[1..];
    }
}
