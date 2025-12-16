using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dfe.SignIn.InternalApi.Swagger;

/// <summary>
/// Schema filter which populates the example value from a <see cref="ExampleValueAttribute"/>
/// annotation on the model property.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SwaggerExampleValueSchemaFilter : ISchemaFilter
{
    private static TAttribute? GetAnnotation<TAttribute>(SchemaFilterContext context)
        where TAttribute : Attribute
    {
        return context.ParameterInfo?.GetCustomAttribute<TAttribute>()
            ?? context.MemberInfo?.GetCustomAttribute<TAttribute>();
    }

    /// <summary>
    /// Apply is called for each parameter
    /// </summary>
    /// <param name="schema">The OpenAPI schema.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Display example value from explicit example value annotation.
        var exampleValueAttribute = GetAnnotation<ExampleValueAttribute>(context);
        if (exampleValueAttribute is not null) {
            schema.Example = new OpenApiString(exampleValueAttribute.Value.ToString());
            return;
        }

        // Display examplem value from recognised pattern.
        string? patternExample = StringPatterns.GetExampleValue(
            GetAnnotation<RegularExpressionAttribute>(context)?.Pattern
        );
        if (patternExample is not null) {
            schema.Example = new OpenApiString(patternExample);
            return;
        }
    }
}
