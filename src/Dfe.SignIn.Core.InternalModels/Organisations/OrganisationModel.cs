using Dfe.SignIn.Core.ExternalModels.Organisations;

namespace Dfe.SignIn.Core.InternalModels.Organisations;

/// <summary>
/// A model representing an organisation in DfE Sign-in.
/// </summary>
public sealed record OrganisationModel()
{
    /// <summary>
    /// Gets the unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the status of the organisation.
    /// </summary>
    public required OrganisationStatus Status { get; init; }

    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the legal name of the organisation.
    /// </summary>
    public string? LegalName { get; init; }

    /// <summary>
    /// Gets the category of the organisation
    /// </summary>
    /// <seealso cref="EstablishmentType"/>
    public OrganisationCategory Category { get; init; }

    /// <summary>
    /// Gets the establishment type of the organisation.
    /// </summary>
    /// <remarks>
    ///   <para>This property is applicable when <see cref="Category"/> is set to
    ///   <see cref="OrganisationCategory.Establishment"/>.</para>
    /// </remarks>
    /// <seealso cref="EstablishmentNumber"/>
    public EstablishmentType? EstablishmentType { get; set; }

    /// <summary>
    /// Gets the establishment number of the organisation.
    /// </summary>
    /// <seealso cref="EstablishmentType"/>
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// Gets the urn of the organisation.
    /// </summary>
    public string? Urn { get; set; }

    /// <summary>
    /// Gets the uid of the organisation.
    /// </summary>
    public string? Uid { get; set; }

    /// <summary>
    /// Gets the upin of the organisation.
    /// </summary>
    public string? Upin { get; set; }

    /// <summary>
    /// Gets the ukprn of the organisation.
    /// </summary>
    public string? Ukprn { get; set; }

    /// <summary>
    /// Gets the closed on of the organisation.
    /// </summary>
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// Gets the address of the organisation.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets the source system of the organisation.
    /// </summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Gets the provider type name of the organisation.
    /// </summary>
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// Gets the provider type code of the organisation.
    /// </summary>
    public int? ProviderTypeCode { get; set; }

    /// <summary>
    /// Gets the GIAS provider type of the organisation.
    /// </summary>
    public string? GIASProviderType { get; set; }

    /// <summary>
    /// Gets the PIMS provider type of the organisation.
    /// </summary>
    public string? PIMSProviderType { get; set; }

    /// <summary>
    /// Gets the PIMS provider type code of the organisation. 
    /// </summary>
    public int? PIMSProviderTypeCode { get; set; }

    /// <summary>
    /// Gets the PIMS status name of the organisation.
    /// </summary>
    public string? PIMSStatusName { get; set; }

    /// <summary>
    /// Gets the PIMS status of the organisation.
    /// </summary>
    public int? PIMSStatus { get; set; }

    /// <summary>
    /// Gets the GIAS status name of the organisation.
    /// </summary>
    public string? GIASStatusName { get; set; }

    /// <summary>
    /// Gets the GIAS status of the organisation.
    /// </summary>
    public int? GIASStatus { get; set; }

    /// <summary>
    /// Gtes the master provider status name of the organisation.
    /// </summary>
    public string? MasterProviderStatusName { get; set; }

    /// <summary>
    /// Gets the master provider status code of the organisation.
    /// </summary>
    public int? MasterProviderStatusCode { get; set; }

    /// <summary>
    /// Gets the opened on of the organisation.
    /// </summary>
    public string? OpenedOn { get; set; }

    /// <summary>
    /// Gets the district administrative name of the organisation.
    /// </summary>
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// Gets the district administrative code of the organisation.
    /// </summary>
    public string? DistrictAdministrativeCode { get; set; }

    /// <summary>
    /// Gets the is on apar of the organisation.
    /// </summary>
    public string? IsOnAPAR { get; set; }
}
