using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApiClient.Organisations.Models;

internal sealed record OrganisationsAssociatedWithUserDto()
{
    public required OrganisationDto Organisation { get; init; }
    public RoleDto? Role { get; init; }
    public Guid[] Approvers { get; init; } = [];
    public Guid[] EndUsers { get; init; } = [];
    public string? NumericIdentifier { get; init; } = null;
    public string? TextIdentifier { get; init; } = null;
}

internal sealed record OrganisationDto()
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    [JsonPropertyName("category")]
    public CategoryDto? Category { get; init; }

    [JsonPropertyName("status")]
    public StatusDto? Status { get; set; }

    [JsonPropertyName("role")]
    public RoleDto? Role { get; init; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("upin")]
    public string? Upin { get; set; }

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    [JsonPropertyName("closedOn")]
    public DateTime? ClosedOn { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; set; }

    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; set; }

    [JsonPropertyName("legacyId")]
    public long? LegacyId { get; set; }

    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; set; }

    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    [JsonPropertyName("providerTypeName")]
    public string? ProviderTypeName { get; set; }

    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; set; }

    [JsonPropertyName("GIASProviderType")]
    public string? GIASProviderType { get; set; }

    [JsonPropertyName("PIMSProviderType")]
    public string? PIMSProviderType { get; set; }

    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PIMSProviderTypeCode { get; set; }

    [JsonPropertyName("PIMSStatusName")]
    public string? PIMSStatusName { get; set; }

    [JsonPropertyName("pimsStatus")]
    public int? PIMSStatus { get; set; }

    [JsonPropertyName("GIASStatusName")]
    public string? GIASStatusName { get; set; }

    [JsonPropertyName("GIASStatus")]
    public int? GIASStatus { get; set; }

    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; set; }

    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; set; }

    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; set; }

    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; set; }

    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnAPAR { get; set; }
}

internal sealed record RoleDto()
{
    [JsonPropertyName("id")]

    public required int Id { get; init; }

    [JsonPropertyName("name")]

    public required string Name { get; init; }
}

internal sealed record StatusDto()
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("tagColor")]
    public required string TagColor { get; init; }
}

internal sealed record CategoryDto()
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}