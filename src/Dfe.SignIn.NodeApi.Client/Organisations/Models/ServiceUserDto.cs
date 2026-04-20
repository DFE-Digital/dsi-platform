namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

public record ServiceUserDto(
    Guid Id,
    int Status,
    ServiceUserOrganisationDto Organisation,
    ServicUserRoleDto Role
);

public record ServiceUserOrganisationDto(
    Guid Id,
    string Name
);

public record ServicUserRoleDto(
    int Id,
    string Name
);
