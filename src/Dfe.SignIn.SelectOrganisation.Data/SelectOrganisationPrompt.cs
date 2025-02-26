namespace Dfe.SignIn.SelectOrganisation.Data;

/// <summary>
/// A model representing the user prompt that is to be presented to the user
/// when they are selecting an organisation.
/// </summary>
public sealed record SelectOrganisationPrompt
{
    /// <summary>
    /// Gets the heading text for the "select organisation" user interface.
    /// </summary>
    /// <remarks>
    ///   <para>The heading text should be short and concise and is likely presented
    ///   in the form of a question.</para>
    /// </remarks>
    public required string Heading { get; init; }

    /// <summary>
    /// Gets the hint text for the "select organisation" user interface.
    /// </summary>
    /// <remarks>
    ///   <para>This is the grey text that appears below the heading text.</para>
    /// </remarks>
    public required string Hint { get; init; }
}
