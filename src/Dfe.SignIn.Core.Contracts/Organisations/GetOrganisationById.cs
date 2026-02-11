using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get an organisation by its unique identifier.
/// </summary>
[AssociatedResponse(typeof(GetOrganisationByIdResponse))]
[Throws(typeof(OrganisationNotFoundException))]
public sealed record GetOrganisationByIdRequest
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationByIdRequest"/>.
/// </summary>
public sealed record GetOrganisationByIdResponse
{
    /// <summary>
    /// A model representing the organisation.
    /// </summary>
    public required Organisation Organisation { get; init; }
}
