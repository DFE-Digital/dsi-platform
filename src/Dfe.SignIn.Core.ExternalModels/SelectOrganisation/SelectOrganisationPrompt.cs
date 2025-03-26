using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// A model representing the user prompt that is to be presented to the user
/// when they are selecting an organisation.
/// </summary>
public sealed record SelectOrganisationPrompt()
{
    /// <summary>
    /// Gets the heading text for the "select organisation" user interface.
    /// </summary>
    /// <remarks>
    ///   <para>The heading text should be short and concise and is likely presented
    ///   in the form of a question.</para>
    /// </remarks>
    [ExampleValue("Which organisation would you like to use?")]
    public required string Heading { get; init; }

    /// <summary>
    /// Gets the hint text for the "select organisation" user interface.
    /// </summary>
    /// <remarks>
    ///   <para>This is the grey text that appears below the heading text.</para>
    /// </remarks>
    [ExampleValue("Select one option.")]
    public string Hint { get; init; } = "Select one option.";
}
