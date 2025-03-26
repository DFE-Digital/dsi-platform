using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dfe.SignIn.Core.Framework;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dfe.SignIn.PublicApi.Swagger;

/// <summary>
/// Schema filter which populates the example value from a <see cref="ExampleValueAttribute"/>
/// annotation on the model property.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SwaggerExampleValueSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Apply is called for each parameter
    /// </summary>
    /// <param name="schema">The OpenAPI schema.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var att = context.ParameterInfo?.GetCustomAttribute<ExampleValueAttribute>()
            ?? context.MemberInfo?.GetCustomAttribute<ExampleValueAttribute>();
        if (att is not null) {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(att.Value.ToString());
        }
    }
}
