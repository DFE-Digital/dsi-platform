using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get all user belogning to an organisations.
/// </summary>
[AssociatedResponse(typeof(GetUsersAssociatedWithOrganisationResponse))]
public sealed record GetUsersAssociatedWithOrganisationRequest
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required string Ukprn { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetUsersAssociatedWithOrganisationRequest"/>.
/// </summary>
public sealed record GetUsersAssociatedWithOrganisationResponse
{
    /// <summary>
    /// An enumerable collection of organisations that are associated with the organisation.
    /// </summary>
    public required IEnumerable<Organisation> Organisations { get; init; }
}
