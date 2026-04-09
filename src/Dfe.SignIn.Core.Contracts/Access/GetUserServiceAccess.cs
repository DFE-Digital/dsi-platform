using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// Request to get a user's access to a specific service within an organisation.
/// </summary>
[AssociatedResponse(typeof(GetUserServiceAccessResponse))]
public sealed record GetUserServiceAccessRequest
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The unique identifier of the service.
    /// </summary>
    public required Guid ServiceId { get; init; }

    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for <see cref="GetUserServiceAccessRequest"/>.
/// </summary>
public sealed record GetUserServiceAccessResponse
{
    /// <summary>
    /// The access record, or <c>null</c> if the user does not have access.
    /// </summary>
    public UserServiceAccess? Access { get; init; }
}

/// <summary>
/// Represents a user's access to a service within an organisation.
/// </summary>
public sealed record UserServiceAccess
{
    /// <summary>The unique identifier of the user.</summary>
    public required Guid UserId { get; init; }

    /// <summary>The unique identifier of the service.</summary>
    public required Guid ServiceId { get; init; }

    /// <summary>The unique identifier of the organisation.</summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>The roles assigned to the user for this service.</summary>
    public required IEnumerable<UserServiceRole> Roles { get; init; }

    /// <summary>The external identifiers associated with this user-service record.</summary>
    public required IEnumerable<UserServiceIdentifier> Identifiers { get; init; }
}

/// <summary>
/// Represents a role assigned to a user for a service.
/// </summary>
public sealed record UserServiceRole
{
    /// <summary>The unique identifier of the role.</summary>
    public required Guid Id { get; init; }

    /// <summary>The name of the role.</summary>
    public required string Name { get; init; }

    /// <summary>The code of the role.</summary>
    public required string Code { get; init; }

    /// <summary>The numeric identifier of the role.</summary>
    public required long NumericId { get; init; }
}

/// <summary>
/// Represents an external identifier associated with a user-service record.
/// </summary>
public sealed record UserServiceIdentifier
{
    /// <summary>The identifier key.</summary>
    public required string Key { get; init; }

    /// <summary>The identifier value.</summary>
    public required string Value { get; init; }
}
