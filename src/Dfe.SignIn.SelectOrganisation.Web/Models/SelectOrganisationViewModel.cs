using Dfe.SignIn.SelectOrganisation.Data;

namespace Dfe.SignIn.SelectOrganisation.Web.Models;

/// <summary>
/// View model for the "Select organisation" user interface.
/// </summary>
public sealed record SelectOrganisationViewModel
{
    /// <summary>
    /// Gets the user prompt content.
    /// </summary>
    public required SelectOrganisationPrompt Prompt { get; init; }

    /// <summary>
    /// Gets the list of organisations that the user can select from.
    /// </summary>
    public required IEnumerable<SelectOrganisationOption> OrganisationOptions { get; init; }
}
