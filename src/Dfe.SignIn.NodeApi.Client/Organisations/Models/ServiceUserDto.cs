namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

/// <summary>
/// Represents a service user, including identity, status, organisation, and role information.
/// </summary>
public record ServiceUserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the service user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the current status of the service user, typically represented as an integer enum value.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the organisation to which the service user belongs.
    /// </summary>
    public ServiceUserOrganisationDto? Organisation { get; set; }

    /// <summary>
    /// Gets or sets the role assigned to the service user within the organisation.
    /// </summary>
    public ServicUserRoleDto? Role { get; set; }
}

/// <summary>
/// Represents an organisation associated with a service user.
/// </summary>
public record ServiceUserOrganisationDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the organisation.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the organisation.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Represents a role assigned to a service user.
/// </summary>
public record ServicUserRoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the role.
    /// </summary>
    public string? Name { get; set; }
}

