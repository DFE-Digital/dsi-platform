using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// A model representing a role in DfE Sign-in.
/// </summary>
public sealed record ApplicationRole
{
    /// <summary>
    /// The unique value that identifies the role.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The parent role.
    /// </summary>
    public ApplicationRole? Parent { get; init; } = null;

    /// <summary>
    /// The code of the role.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The name of the role.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The numeric identifier of the role.
    /// </summary>
    public required int NumericId { get; init; }

    /// <summary>
    /// A value indicating the status of the role.
    /// </summary>
    [EnumDataType(typeof(ApplicationRoleStatus))]
    public required ApplicationRoleStatus Status { get; init; }
}

/// <summary>
/// Represents the status of a role.
/// </summary>
public enum ApplicationRoleStatus
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
