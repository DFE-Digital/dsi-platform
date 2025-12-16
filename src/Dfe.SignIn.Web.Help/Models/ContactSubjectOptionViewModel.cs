namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// Represents the view model for a contact subject option.
/// </summary>
public sealed record ContactSubjectOptionViewModel
{
    /// <summary>
    /// The unique value that identifies the subject.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// The user friendly text of the subject.
    /// </summary>
    public required string Text { get; init; }
}
