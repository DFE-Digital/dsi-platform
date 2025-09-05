using Dfe.SignIn.Core.Public.SelectOrganisation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dfe.SignIn.Web.SelectOrganisation.Models;

/// <summary>
/// View model for the "Select organisation" user interface.
/// </summary>
public sealed class SelectOrganisationViewModel
{
    /// <summary>
    /// Gets the "Sign out" URL (if any).
    /// </summary>
    [ValidateNever]
    public string? SignOutUrl { get; init; }

    /// <summary>
    /// Gets the user prompt content.
    /// </summary>
    [ValidateNever]
    public required SelectOrganisationPrompt Prompt { get; init; }

    /// <summary>
    /// Gets the list of organisations that the user can select from.
    /// </summary>
    [ValidateNever]
    public required IEnumerable<SelectOrganisationOption> OrganisationOptions { get; init; }

    /// <summary>
    /// Gets a value indicating if the user can cancel selection.
    /// </summary>
    [ValidateNever]
    public required bool AllowCancel { get; init; }

    /// <summary>
    /// Gets or sets the unique ID of the selected organisation.
    /// </summary>
    public Guid? SelectedOrganisationIdInput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the user has cancelled.
    /// </summary>
    /// <remarks>
    ///   <para>A value of "1" indicates that the action was cancelled.</para>
    /// </remarks>
    public string? CancelAction { get; set; } = null;
}
