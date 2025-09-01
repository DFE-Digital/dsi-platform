using System.Text.Json.Serialization;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

internal abstract record OrganisationDto()
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("upin")]
    public string? Upin { get; set; }

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

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

    protected OrganisationModel MapToOrganisationModel(
        OrganisationStatus organisationStatus,
        string organisationCategory,
        string? establishmentType)
    {

        EstablishmentType? actualEstablishmentType = (organisationCategory == OrganisationConstants.CategoryId_Establishment && establishmentType != null)
            ? EnumHelpers.MapEnum<EstablishmentType>(establishmentType)
            : null;

        return new OrganisationModel {
            Id = this.Id,
            Status = organisationStatus,
            Name = this.Name,
            LegalName = this.LegalName,
            Category = EnumHelpers.MapEnum<OrganisationCategory>(organisationCategory),
            EstablishmentType = actualEstablishmentType,
            EstablishmentNumber = this.EstablishmentNumber,
            Urn = this.Urn,
            Uid = this.Uid,
            Upin = this.Upin,
            Ukprn = this.Ukprn,
            ClosedOn = this.ClosedOn,
            Address = this.Address,
            SourceSystem = this.SourceSystem,
            ProviderTypeName = this.ProviderTypeName,
            ProviderTypeCode = this.ProviderTypeCode,
            GIASProviderType = this.GIASProviderType,
            PIMSProviderType = this.PIMSProviderType,
            PIMSProviderTypeCode = this.PIMSProviderTypeCode,
            PIMSStatusName = this.PIMSStatusName,
            PIMSStatus = this.PIMSStatus,
            GIASStatus = this.GIASStatus,
            GIASStatusName = this.GIASStatusName,
            MasterProviderStatusCode = this.MasterProviderStatusCode,
            MasterProviderStatusName = this.MasterProviderStatusName,
            OpenedOn = this.OpenedOn,
            DistrictAdministrativeName = this.DistrictAdministrativeName,
            DistrictAdministrativeCode = this.DistrictAdministrativeCode,
            IsOnAPAR = this.IsOnAPAR,
        };
    }
}
