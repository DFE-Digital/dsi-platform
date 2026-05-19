using System.Text.Json.Serialization;

namespace Dfe.SignIn.PublicApi.Models;

/// <summary>
/// Response model representing a user and their associated organisations and services.
/// Property names are aligned with the Node.js API contract.
/// </summary>
public class GetUserOrganisationServicesResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's status identifier.
    /// </summary>
    [JsonPropertyName("userStatus")]
    public int UserStatus { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's family (last) name.
    /// </summary>
    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; }

    /// <summary>
    /// Gets or sets the user's given (first) name.
    /// </summary>
    [JsonPropertyName("givenName")]
    public string GivenName { get; set; }

    /// <summary>
    /// Gets or sets the collection of organisations associated with the user.
    /// </summary>
    [JsonPropertyName("organisations")]
    public IEnumerable<OrganisationDto> Organisations { get; set; } = [];
}

/// <summary>
/// Represents an organisation and its associated details and services.
/// </summary>
public class OrganisationDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the organisation.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the organisation.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the organisation category details.
    /// </summary>
    [JsonPropertyName("category")]
    public CategoryDto Category { get; set; }

    /// <summary>
    /// Gets or sets the Unique Reference Number (URN).
    /// </summary>
    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (UID) of the organisation.
    /// </summary>
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    /// <summary>
    /// Gets or sets the UK Provider Reference Number (UKPRN).
    /// </summary>
    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    /// <summary>
    /// Gets or sets the establishment number of the organisation.
    /// </summary>
    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// Gets or sets the organisation status details.
    /// </summary>
    [JsonPropertyName("status")]
    public StatusDto Status { get; set; }

    /// <summary>
    /// Gets or sets the date the organisation was closed.
    /// </summary>
    [JsonPropertyName("closedOn")]
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// Gets or sets the address of the organisation.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the organisation's contact telephone number.
    /// </summary>
    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    /// <summary>
    /// Gets or sets the statutory lowest age for the organisation.
    /// </summary>
    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; set; }

    /// <summary>
    /// Gets or sets the statutory highest age for the organisation.
    /// </summary>
    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; set; }

    /// <summary>
    /// Gets or sets the legacy identifier of the organisation.
    /// </summary>
    [JsonPropertyName("legacyId")]
    public string? LegacyId { get; set; }

    /// <summary>
    /// Gets or sets the company registration number.
    /// </summary>
    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; set; }

    /// <summary>
    /// Gets or sets the provider profile identifier.
    /// </summary>
    [JsonPropertyName("ProviderProfileID")]
    public string? ProviderProfileID { get; set; }

    /// <summary>
    /// Gets or sets the Unique Provider Identification Number (UPIN).
    /// </summary>
    [JsonPropertyName("UPIN")]
    public string? UPIN { get; set; }

    /// <summary>
    /// Gets or sets the PIMS provider type.
    /// </summary>
    [JsonPropertyName("PIMSProviderType")]
    public string? PIMSProviderType { get; set; }

    /// <summary>
    /// Gets or sets the PIMS status of the organisation.
    /// </summary>
    [JsonPropertyName("PIMSStatus")]
    public string? PIMSStatus { get; set; }

    /// <summary>
    /// Gets or sets the district administrative name.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// Gets or sets the organisation opening date (as a string).
    /// </summary>
    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; set; }

    /// <summary>
    /// Gets or sets the source system from which this organisation data originates.
    /// </summary>
    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Gets or sets the provider type name.
    /// </summary>
    [JsonPropertyName("ProviderTypeName")]
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// Gets or sets the GIAS provider type.
    /// </summary>
    [JsonPropertyName("GIASProviderType")]
    public string? GIASProviderType { get; set; }

    /// <summary>
    /// Gets or sets the PIMS provider type code.
    /// </summary>
    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PIMSProviderTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the collection of services associated with the organisation.
    /// </summary>
    [JsonPropertyName("services")]
    public IEnumerable<ServiceDto> Services { get; set; } = [];

    /// <summary>
    /// Gets or sets the organisation role identifier for the user.
    /// </summary>
    [JsonPropertyName("orgRoleId")]
    public int OrgRoleId { get; set; }

    /// <summary>
    /// Gets or sets the organisation role name for the user.
    /// </summary>
    [JsonPropertyName("orgRoleName")]
    public string? OrgRoleName { get; set; }
}

/// <summary>
/// Represents a category assigned to an organisation.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents the status of an organisation.
/// </summary>
public class StatusDto
{
    /// <summary>
    /// Gets or sets the status identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the status name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents a service associated with an organisation.
/// </summary>
public class ServiceDto
{
    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the service.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of roles assigned within the service.
    /// </summary>
    [JsonPropertyName("roles")]
    public IEnumerable<RoleDto> Roles { get; set; } = [];
}

/// <summary>
/// Represents a role assigned to a user within a service.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the code associated with the role.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }
}
