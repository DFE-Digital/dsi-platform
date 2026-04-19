namespace Dfe.SignIn.NodeApi.Client.Access.Models;

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

public record UserRoleDto(
    Guid Id,
    string Name,
    string Code,
    string NumericId,
    UserRoleStatusDto Status
);

public record UserRoleStatusDto(
    int Id
);

public record Identifier(); // Empty object in your JSON
