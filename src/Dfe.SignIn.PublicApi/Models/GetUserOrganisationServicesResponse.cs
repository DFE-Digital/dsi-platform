using System.Text.Json.Serialization;

namespace Dfe.SignIn.PublicApi.Models;

/// <summary>
/// Property names are set to match NodeJs api.
/// </summary>
public class GetUserOrganisationServicesResponse
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("userStatus")]
    public int UserStatus { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; }

    [JsonPropertyName("givenName")]
    public string GivenName { get; set; }

    [JsonPropertyName("organisations")]
    public IEnumerable<OrganisationDto> Organisations { get; set; } = [];
}

public class OrganisationDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public CategoryDto Category { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    [JsonPropertyName("status")]
    public StatusDto Status { get; set; }

    [JsonPropertyName("closedOn")]
    public DateOnly? ClosedOn { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; set; }

    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; set; }

    [JsonPropertyName("legacyId")]
    public string? LegacyId { get; set; }

    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; set; }

    [JsonPropertyName("ProviderProfileID")]
    public string? ProviderProfileID { get; set; }

    [JsonPropertyName("UPIN")]
    public string? UPIN { get; set; }

    [JsonPropertyName("PIMSProviderType")]
    public string? PIMSProviderType { get; set; }

    [JsonPropertyName("PIMSStatus")]
    public string? PIMSStatus { get; set; }

    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; set; }

    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    [JsonPropertyName("ProviderTypeName")]
    public string? ProviderTypeName { get; set; }

    [JsonPropertyName("GIASProviderType")]
    public string? GIASProviderType { get; set; }

    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PIMSProviderTypeCode { get; set; }

    [JsonPropertyName("services")]
    public IEnumerable<ServiceDto> Services { get; set; } = [];

    [JsonPropertyName("orgRoleId")]
    public int OrgRoleId { get; set; }

    [JsonPropertyName("orgRoleName")]
    public string? OrgRoleName { get; set; }
}

public class CategoryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class StatusDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class ServiceDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("roles")]
    public IEnumerable<RoleDto> Roles { get; set; } = [];
}

public class RoleDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}
