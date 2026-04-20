using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

/// <summary>
/// Contains details about an organisation.
/// </summary>
public record OrganisationDto()
{
    /// <summary>
    /// The unique identifier for the organisation.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    /// <summary>
    /// The name of the organisation.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The legal name of the organisation.
    /// </summary>
    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    /// <summary>
    /// The establishment number.
    /// </summary>
    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// The Unique Reference Number (URN).
    /// </summary>
    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    /// <summary>
    /// The Unique Identifier (UID).
    /// </summary>
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    /// <summary>
    /// The Unique Pupil Number (UPIN).
    /// </summary>
    [JsonPropertyName("upin")]
    public string? Upin { get; set; }

    /// <summary>
    /// The UK Provider Reference Number (UKPRN).
    /// </summary>
    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    /// <summary>
    /// The date the organisation was closed.
    /// </summary>
    [JsonPropertyName("closedOn")]
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// The address of the organisation.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// The source system for the organisation data.
    /// </summary>
    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    /// <summary>
    /// The name of the provider type.
    /// </summary>
    [JsonPropertyName("providerTypeName")]
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// The code for the provider type.
    /// </summary>
    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; set; }

    /// <summary>
    /// The GIAS provider type.
    /// </summary>
    [JsonPropertyName("GIASProviderType")]
    public string? GIASProviderType { get; set; }

    /// <summary>
    /// The PIMS provider type.
    /// </summary>
    [JsonPropertyName("PIMSProviderType")]
    public string? PIMSProviderType { get; set; }

    /// <summary>
    /// The PIMS provider type code.
    /// </summary>
    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PIMSProviderTypeCode { get; set; }

    /// <summary>
    /// The PIMS status name.
    /// </summary>
    [JsonPropertyName("PIMSStatusName")]
    public string? PIMSStatusName { get; set; }

    /// <summary>
    /// The PIMS status code.
    /// </summary>
    [JsonPropertyName("pimsStatus")]
    public int? PIMSStatus { get; set; }

    /// <summary>
    /// The GIAS status name.
    /// </summary>
    [JsonPropertyName("GIASStatusName")]
    public string? GIASStatusName { get; set; }

    /// <summary>
    /// The GIAS status code.
    /// </summary>
    [JsonPropertyName("GIASStatus")]
    public int? GIASStatus { get; set; }

    /// <summary>
    /// The master provider status name.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; set; }

    /// <summary>
    /// The master provider status code.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; set; }

    /// <summary>
    /// The date the organisation was opened.
    /// </summary>
    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; set; }

    /// <summary>
    /// The district administrative name.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// The district administrative code.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; set; }

    /// <summary>
    /// Indicates whether the organisation is on APAR.
    /// </summary>
    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnAPAR { get; set; }

    public Organisation MapToOrganisation(
        OrganisationStatus organisationStatus,
        string organisationCategory,
        string? establishmentType)
    {

        EstablishmentType? actualEstablishmentType = (organisationCategory == OrganisationConstants.CategoryId_Establishment && establishmentType != null)
            ? EnumHelpers.MapEnum<EstablishmentType>(establishmentType)
            : null;

        return new Organisation {
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
            GiasProviderType = this.GIASProviderType,
            PimsProviderType = this.PIMSProviderType,
            PimsProviderTypeCode = this.PIMSProviderTypeCode,
            PimsStatusName = this.PIMSStatusName,
            PimsStatus = this.PIMSStatus,
            GiasStatus = this.GIASStatus,
            GiasStatusName = this.GIASStatusName,
            MasterProviderStatusCode = this.MasterProviderStatusCode,
            MasterProviderStatusName = this.MasterProviderStatusName,
            OpenedOn = this.OpenedOn,
            DistrictAdministrativeName = this.DistrictAdministrativeName,
            DistrictAdministrativeCode = this.DistrictAdministrativeCode,
            IsOnApar = this.IsOnAPAR,
        };
    }
}
