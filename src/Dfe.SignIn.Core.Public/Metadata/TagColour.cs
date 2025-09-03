namespace Dfe.SignIn.Core.Public.Metadata;

/// <summary>
/// Specifies a tag colour for the annotated item.
/// </summary>
/// <param name="colour">The colour of the tag.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class TagColourAttribute(TagColour colour) : Attribute
{
    /// <summary>
    /// Gets the colour of the tag.
    /// </summary>
    public TagColour Colour => colour;
}

/// <summary>
/// Specifies the tag color.
/// </summary>
/// <seealso cref="TagColourAttribute"/>
public enum TagColour
{
    /// <summary>
    /// Represents a tag colour of grey.
    /// </summary>
    Grey,

    /// <summary>
    /// Represents a tag colour of green.
    /// </summary>
    Green,

    /// <summary>
    /// Represents a tag colour of red.
    /// </summary>
    Red,

    /// <summary>
    /// Represents a tag colour of orange.
    /// </summary>
    Orange,

    /// <summary>
    /// Represents a tag colour of blue.
    /// </summary>
    Blue,

    /// <summary>
    /// Represents a tag colour of purple.
    /// </summary>
    Purple,
}
