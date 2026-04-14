using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;

namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get a user's access details for a service within an organisation.
/// Corresponds to the public API endpoint: GET /services/{sid}/organisations/{oid}/users/{uid}
/// </summary>
[AssociatedResponse(typeof(GetUserAccessToServiceResponse))]
public sealed record GetUserAccessToServiceRequest
{
    /// <summary>
    /// The unique identifier of the service.
    /// </summary>
    public required Guid ServiceId { get; init; }

    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for <see cref="GetUserAccessToServiceRequest"/>.
/// </summary>
public sealed record GetUserAccessToServiceResponse
{
    /// <summary>The unique identifier of the user.</summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The legacy numeric identifier for the user within the organisation.
    /// </summary>
    public long? UserLegacyNumericId { get; init; }

    /// <summary>
    /// The legacy text identifier for the user within the organisation.
    /// </summary>
    public string? UserLegacyTextId { get; init; }

    /// <summary>The unique identifier of the service.</summary>
    public required Guid ServiceId { get; init; }

    /// <summary>The unique identifier of the organisation.</summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// The legacy identifier of the organisation.
    /// </summary>
    public long? OrganisationLegacyId { get; init; }

    /// <summary>
    /// Indicates whether the organisation is on APAR.
    /// </summary>
    public string? OrganisationIsOnApar { get; init; }

    /// <summary>The roles assigned to the user for this service.</summary>
    public required IEnumerable<UserServiceRole> Roles { get; init; }

    /// <summary>The external identifiers associated with this user-service record.</summary>
    public required IEnumerable<UserServiceIdentifier> Identifiers { get; init; }
}
