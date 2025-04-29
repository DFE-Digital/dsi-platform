namespace Dfe.SignIn.Core.ExternalModels.Metadata;

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
    /// <exclude/>
    Grey,
    /// <exclude/>
    Green,
    /// <exclude/>
    Red,
    /// <exclude/>
    Orange,
    /// <exclude/>
    Blue,
    /// <exclude/>
    Purple,
}
