using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.PublicApi.Models;

public class GetUserOrganisationServicesResponse
{

    public Guid UserId { get; set; }
    public int UserStatus { get; set; }
    public string Email { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }

    public List<OrganisationDto> Organisations { get; set; } = [];

}

public class OrganisationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public CategoryDto Category { get; set; }
    public StatusDto Status { get; set; }

    public string? Urn { get; set; }
    public string? Uid { get; set; }
    public string? Ukprn { get; set; }
    public string? EstablishmentNumber { get; set; }

    public DateOnly? ClosedOn { get; set; }

    public string? Address { get; set; }
    public string? Telephone { get; set; }

    public int? StatutoryLowAge { get; set; }
    public int? StatutoryHighAge { get; set; }

    public long? LegacyId { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string? ProviderProfileID { get; set; }
    public string? UPIN { get; set; }

    public string? PIMSProviderType { get; set; }
    public string? PIMSStatus { get; set; }
    public string? DistrictAdministrativeName { get; set; }

    public string? OpenedOn { get; set; }

    public string? SourceSystem { get; set; }
    public string? ProviderTypeName { get; set; }
    public string? GIASProviderType { get; set; }
    public int? PIMSProviderTypeCode { get; set; }

    public List<ServiceDto> Services { get; set; } = [];

    public int OrgRoleId { get; set; }
    public string OrgRoleName { get; set; }
}

public class CategoryDto
{
    public string? Id { get; set; }
    public OrganisationCategory? Name { get; set; }
}

public class StatusDto
{
    public int Id { get; set; }
    public OrganisationStatus? Name { get; set; }
}

public class ServiceDto
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public List<RoleDto> Roles { get; set; } = [];
}

public class RoleDto
{
    public string Name { get; set; }
    public string Code { get; set; }
}
