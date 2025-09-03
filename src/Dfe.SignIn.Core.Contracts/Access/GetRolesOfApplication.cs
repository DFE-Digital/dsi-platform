namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// Request to get the roles that are associated with an application.
/// </summary>
public sealed record GetRolesOfApplicationRequest
{
    /// <summary>
    /// Gets the unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetRolesOfApplicationRequest"/>.
/// </summary>
public sealed record GetRolesOfApplicationResponse
{
    /// <summary>
    /// Gets the enumerable collection of application roles.
    /// </summary>
    public required IEnumerable<ApplicationRole> Roles { get; init; }
}
