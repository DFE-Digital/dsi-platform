namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// Represents the view model for a contact subject option.
/// </summary>
public sealed record ContactSubjectOptionViewModel
{
    /// <summary>
    /// Gets the unique value that identifies the subject.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets the user friendly text of the subject.
    /// </summary>
    public required string Text { get; init; }
}
