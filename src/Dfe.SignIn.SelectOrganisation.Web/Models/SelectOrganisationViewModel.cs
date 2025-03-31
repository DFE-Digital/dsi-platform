using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.SelectOrganisation.Web.Models;

/// <summary>
/// View model for the "Select organisation" user interface.
/// </summary>
public sealed class SelectOrganisationViewModel
{
    /// <summary>
    /// Gets the user prompt content.
    /// </summary>
    public required SelectOrganisationPrompt Prompt { get; init; }

    /// <summary>
    /// Gets the list of organisations that the user can select from.
    /// </summary>
    public required IEnumerable<SelectOrganisationOption> OrganisationOptions { get; init; }

    /// <summary>
    /// Gets or sets the unique ID of the selected organisation.
    /// </summary>
    public Guid? SelectedOrganisationId { get; set; }
}
