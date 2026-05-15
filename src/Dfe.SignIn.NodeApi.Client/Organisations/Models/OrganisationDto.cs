using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

/// <summary>
/// Data transfer object representing an organisation and its associated identifiers,
/// status information, and classification details.
/// </summary>
public record OrganisationDto
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the display name of the organisation.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the registered legal name of the organisation, if different from the display name.
    /// </summary>
    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    /// <summary>
    /// Gets or sets the establishment number assigned to the organisation.
    /// </summary>
    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// Gets or sets the Unique Reference Number (URN).
    /// </summary>
    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier used by internal systems.
    /// </summary>
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    /// <summary>
    /// Gets or sets the Unique Provider Identification Number (UPIN).
    /// </summary>
    [JsonPropertyName("upin")]
    public string? Upin { get; set; }

    /// <summary>
    /// Gets or sets the UK Provider Reference Number (UKPRN).
    /// </summary>
    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    /// <summary>
    /// Gets or sets the date on which the organisation was closed, if applicable.
    /// </summary>
    [JsonPropertyName("closedOn")]
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// Gets or sets the registered address of the organisation.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the source system from which the organisation data originated.
    /// </summary>
    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Gets or sets the provider type name as supplied by the source system.
    /// </summary>
    [JsonPropertyName("providerTypeName")]
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// Gets or sets the provider type code.
    /// </summary>
    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the provider type as defined by GIAS.
    /// </summary>
    [JsonPropertyName("GIASProviderType")]
    public string? GIASProviderType { get; set; }

    /// <summary>
    /// Gets or sets the provider type as defined by PIMS.
    /// </summary>
    [JsonPropertyName("PIMSProviderType")]
    public string? PIMSProviderType { get; set; }

    /// <summary>
    /// Gets or sets the provider type code as defined by PIMS.
    /// </summary>
    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PIMSProviderTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the PIMS status name.
    /// </summary>
    [JsonPropertyName("PIMSStatusName")]
    public string? PIMSStatusName { get; set; }

    /// <summary>
    /// Gets or sets the PIMS status code.
    /// </summary>
    [JsonPropertyName("pimsStatus")]
    public int? PIMSStatus { get; set; }

    /// <summary>
    /// Gets or sets the GIAS status name.
    /// </summary>
    [JsonPropertyName("GIASStatusName")]
    public string? GIASStatusName { get; set; }

    /// <summary>
    /// Gets or sets the GIAS status code.
    /// </summary>
    [JsonPropertyName("GIASStatus")]
    public int? GIASStatus { get; set; }

    /// <summary>
    /// Gets or sets the master provider status name.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; set; }

    /// <summary>
    /// Gets or sets the master provider status code.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the date on which the organisation opened.
    /// </summary>
    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; set; }

    /// <summary>
    /// Gets or sets the administrative district name.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// Gets or sets the administrative district code.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the organisation is on APAR.
    /// </summary>
    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnAPAR { get; set; }

    /// <summary>
    /// Maps this DTO to a domain <see cref="Organisation"/> instance.
    /// </summary>
    /// <param name="organisationStatus">
    /// The status to assign to the organisation.
    /// </param>
    /// <param name="organisationCategory">
    /// The category identifier used to determine the organisation type.
    /// </param>
    /// <param name="establishmentType">
    /// The establishment type string to be mapped when the category is an establishment.
    /// </param>
    /// <returns>
    /// A populated <see cref="Organisation"/> domain object.
    /// </returns>
    protected Organisation MapToOrganisation(
        OrganisationStatus organisationStatus,
        string organisationCategory,
        string? establishmentType)
    {
        EstablishmentType? actualEstablishmentType =
            (organisationCategory == OrganisationConstants.CategoryId_Establishment &&
             establishmentType != null)
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
