using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request to get the legacy numeric and text identifiers for a user within an organisation.
/// These identifiers are stored on the user_organisation record.
/// </summary>
[AssociatedResponse(typeof(GetUserOrganisationIdentifiersResponse))]
public sealed record GetUserOrganisationIdentifiersRequest
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for <see cref="GetUserOrganisationIdentifiersRequest"/>.
/// </summary>
public sealed record GetUserOrganisationIdentifiersResponse
{
    /// <summary>
    /// The legacy numeric identifier for the user within the organisation.
    /// May be <c>null</c> if not assigned.
    /// </summary>
    public long? NumericIdentifier { get; init; }

    /// <summary>
    /// The legacy text identifier for the user within the organisation.
    /// May be <c>null</c> if not assigned.
    /// </summary>
    public string? TextIdentifier { get; init; }
}
