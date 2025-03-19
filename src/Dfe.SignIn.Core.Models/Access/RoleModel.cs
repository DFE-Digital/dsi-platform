using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Models.Access;

/// <summary>
/// A model representing a role in DfE Sign-in.
/// </summary>
public sealed record RoleModel()
{
    /// <summary>
    /// Gets the unique value that identifies the role.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the parent role.
    /// </summary>
    public RoleModel? Parent { get; init; } = null;

    /// <summary>
    /// Gets the code of the role.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the name of the role.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the numeric identifier of the role.
    /// </summary>
    public required int NumericId { get; init; }

    /// <summary>
    /// Gets a value indicating the status of the role.
    /// </summary>
    [EnumDataType(typeof(RoleStatus))]
    public required RoleStatus Status { get; init; }
}

/// <summary>
/// Represents the status of a role.
/// </summary>
public enum RoleStatus
{
    /// <summary>
    /// Indicates that a role is inactive.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// Indicates that a role is active.
    /// </summary>
    Active = 1,
}
