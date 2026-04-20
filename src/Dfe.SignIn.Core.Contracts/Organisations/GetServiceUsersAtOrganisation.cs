using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get all user belogning to an organisations.
/// </summary>
[AssociatedResponse(typeof(GetServiceUsersAtOrganisationResponse))]
public sealed record GetServiceUsersAtOrganisationRequest
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// The unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetServiceUsersAtOrganisationRequest"/>.
/// </summary>
public record GetServiceUsersAtOrganisationResponse(
   IOrderedEnumerable<Guid> UserIds
);
