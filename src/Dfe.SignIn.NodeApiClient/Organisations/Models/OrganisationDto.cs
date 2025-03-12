using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApiClient.Organisations.Models;

internal abstract record OrganisationDto()
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

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
    public DateOnly? ClosedOn { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

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
