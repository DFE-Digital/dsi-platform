using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.NodeApi.Client.Access.Models;

/// <summary>
/// Represents a user and their roles within a specific organisation and service.
/// </summary>
/// <param name="UserId">
/// The unique identifier of the user.
/// </param>
/// <param name="UserLegacyNumericId">
/// The legacy numeric identifier associated with the user.
/// </param>
/// <param name="UserLegacyTextId">
/// The legacy text-based identifier associated with the user.
/// </param>
/// <param name="ServiceId">
/// The unique identifier of the service the user belongs to.
/// </param>
/// <param name="OrganisationId">
/// The unique identifier of the organisation.
/// </param>
/// <param name="OrganisationLegacyId">
/// The legacy identifier associated with the organisation.
/// </param>
/// <param name="Roles">
/// The collection of roles assigned to the user within the organisation.
/// </param>
/// <param name="Identifiers">
/// Additional identifiers associated with the user.
/// </param>
[ExcludeFromCodeCoverage]
public record ApplicationUserRoleDto(
    Guid UserId,
    string UserLegacyNumericId,
    string UserLegacyTextId,
    Guid ServiceId,
    Guid OrganisationId,
    string OrganisationLegacyId,
    List<UserRoleDto> Roles,
    List<Identifier> Identifiers
);

/// <summary>
/// Represents a role that can be assigned to a user.
/// </summary>
/// <param name="Id">
/// The unique identifier of the role.
/// </param>
/// <param name="Name">
/// The display name of the role.
/// </param>
/// <param name="Code">
/// The system code representing the role.
/// </param>
/// <param name="NumericId">
/// The legacy numeric identifier of the role.
/// </param>
/// <param name="Status">
/// The current status of the role.
/// </param>
[ExcludeFromCodeCoverage]
public record UserRoleDto(
    Guid Id,
    string Name,
    string Code,
    string NumericId,
    UserRoleStatusDto Status
);

/// <summary>
/// Represents the status of a user role.
/// </summary>
/// <param name="Id">
/// The numeric identifier of the role status.
/// </param>
[ExcludeFromCodeCoverage]
public record UserRoleStatusDto(
    int Id
);

/// <summary>
/// Represents an identifier associated with a user or organisation.
/// </summary>
[ExcludeFromCodeCoverage]
public record Identifier();
