using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Annotates a property with an example value.
/// </summary>
/// <remarks>
///   <para>The example value can be presented in API documentation and/or
///   user interfaces such as Swagger.</para>
/// </remarks>
/// <param name="value">The example value.</param>
[ExcludeFromCodeCoverage]
public sealed class ExampleValueAttribute(string value) : Attribute
{
    /// <summary>
    /// Gets the example value.
    /// </summary>
    public string Value => value;
}
