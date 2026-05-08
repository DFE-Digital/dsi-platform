using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// Request to get the roles that are associated with an application.
/// </summary>
[AssociatedResponse(typeof(GetApplicationRolesResponse))]
public sealed record GetApplicationRolesRequest
{
    /// <summary>
    /// The unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetApplicationRolesRequest"/>.
/// </summary>
public sealed record GetApplicationRolesResponse
{
    /// <summary>
    /// An enumerable collection of application roles.
    /// </summary>
    public required IEnumerable<ApplicationRole> Roles { get; init; }
}
