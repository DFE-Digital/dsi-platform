using System.ComponentModel;
using System.Reflection;

namespace Dfe.SignIn.Core.PublicModels.Metadata;

/// <summary>
/// Helpers functionality for metadata annotations.
/// </summary>
public static class AnnotationHelpers
{
    /// <summary>
    /// Gets description annotation from an enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>
    ///   <para>The description when present; otherwise, a value of null.</para>
    /// </returns>
    public static string? GetDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description;
    }

    /// <summary>
    /// Gets tag colour annotation from an enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>
    ///   <para>The tag colour when present; otherwise, a value of null.</para>
    /// </returns>
    public static TagColour? GetTagColour(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var tagColorAttribute = fieldInfo?.GetCustomAttribute<TagColourAttribute>();
        return tagColorAttribute?.Colour;
    }
}
