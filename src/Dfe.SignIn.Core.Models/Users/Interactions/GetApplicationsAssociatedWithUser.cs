namespace Dfe.SignIn.Core.Models.Users.Interactions;

/// <summary>
/// Request to get all the applications that are associated with a user.
/// </summary>
public sealed record GetApplicationsAssociatedWithUserRequest()
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetApplicationsAssociatedWithUserRequest"/>.
/// </summary>
public sealed record GetApplicationsAssociatedWithUserResponse()
{
    /// <summary>
    /// Gets the enumerable collection of user service mappings.
    /// </summary>
    public required IEnumerable<UserServiceMappingModel> UserServiceMappings { get; init; }
}
