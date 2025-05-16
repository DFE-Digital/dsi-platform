using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Core.InternalModels.SelectOrganisation;

/// <summary>
/// A model representing a "select organisation" session.
/// </summary>
public sealed record SelectOrganisationSessionData()
{
    /// <summary>
    /// Gets the date/time that the session was created.
    /// </summary>
    public required DateTime Created { get; init; }

    /// <summary>
    /// Gets the date/time that the session expires.
    /// </summary>
    public required DateTime Expires { get; init; }

    /// <summary>
    /// Gets the unique client ID of the relying party that initiated the
    /// "select organisation" request on behalf of the user.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the unique identifier representing the user in DfE Sign-in.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the prompt that will be presented to the user when they are selecting
    /// an organisation.
    /// </summary>
    public required SelectOrganisationPrompt Prompt { get; init; }

    /// <summary>
    /// Gets the enumerable collection of organisations that will be presented
    /// to the user as options when they are making their selection.
    /// </summary>
    public required IEnumerable<SelectOrganisationOption> OrganisationOptions { get; init; }

    /// <summary>
    /// Gets a value indicating if the user can cancel selection.
    /// </summary>
    public required bool AllowCancel { get; init; }

    /// <summary>
    /// Gets the callback URL which is invoked when the user makes a selection;
    /// when there is no selection to be made; or when an error has occurred.
    /// </summary>
    public required Uri CallbackUrl { get; init; }
}
