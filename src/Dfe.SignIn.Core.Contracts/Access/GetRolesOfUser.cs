using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// Request to get the roles that are associated with an application user at an organisation.
/// </summary>
[AssociatedResponse(typeof(GetRolesOfUserResponse))]
public sealed record GetRolesOfUserRequest
{
    /// <summary>
    /// The unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }

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
/// Response model for request <see cref="GetRolesOfUserRequest"/>.
/// </summary>
public sealed record GetRolesOfUserResponse
{
    /// <summary>
    /// An enumerable collection of roles.
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
