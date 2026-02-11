using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request to get all the applications that are associated with a user.
/// </summary>
[AssociatedResponse(typeof(GetApplicationsAssociatedWithUserResponse))]
public sealed record GetApplicationsAssociatedWithUserRequest
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetApplicationsAssociatedWithUserRequest"/>.
/// </summary>
public sealed record GetApplicationsAssociatedWithUserResponse
{
    /// <summary>
    /// An enumerable collection of user application mappings.
    /// </summary>
    public required IEnumerable<UserApplicationMapping> UserApplicationMappings { get; init; }
}
