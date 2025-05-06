using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Web.SelectOrganisation.Models;

/// <summary>
/// View model for the "Select organisation" user interface.
/// </summary>
public sealed class SelectOrganisationViewModel
{
    /// <summary>
    /// Gets the "Sign out" URL (if any).
    /// </summary>
    public string? SignOutUrl { get; init; }

    /// <summary>
    /// Gets the user prompt content.
    /// </summary>
    public required SelectOrganisationPrompt Prompt { get; init; }

    /// <summary>
    /// Gets the list of organisations that the user can select from.
    /// </summary>
    public required IEnumerable<SelectOrganisationOption> OrganisationOptions { get; init; }

    /// <summary>
    /// Gets a value indicating if the user can cancel selection.
    /// </summary>
    public required bool AllowCancel { get; init; }

    /// <summary>
    /// Gets or sets the unique ID of the selected organisation.
    /// </summary>
    public Guid? SelectedOrganisationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the user has cancelled.
    /// </summary>
    public string? Cancel { get; set; } = null;
}
