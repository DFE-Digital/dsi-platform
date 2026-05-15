namespace Dfe.SignIn.PublicApi.Contracts.Applications;

/// <summary>
/// Represents a role associated with an application.
/// </summary>
public sealed record ApplicationRoleDto
{
    /// <summary>
    /// The role name.
    /// </summary>
    /// <example>DSI Child One</example>
    public required string Name { get; init; }

    /// <summary>
    /// The role code.
    /// </summary>
    /// <example>DSI_Child_One</example>
    public required string Code { get; init; }

    /// <summary>
    /// The status of the role.
    /// </summary>
    /// <example>Active</example>
    public required string Status { get; init; }
}
