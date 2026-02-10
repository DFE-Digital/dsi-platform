using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// A model representing an organisation in DfE Sign-in.
/// </summary>
public sealed record Organisation
{
    /// <summary>
    /// The unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The status of the organisation.
    /// </summary>
    public required OrganisationStatus Status { get; init; }

    /// <summary>
    /// The name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The legal name of the organisation.
    /// </summary>
    public string? LegalName { get; init; }

    /// <summary>
    /// The category of the organisation
    /// </summary>
    /// <seealso cref="EstablishmentType"/>
    public OrganisationCategory Category { get; init; }

    /// <summary>
    /// The establishment type of the organisation.
    /// </summary>
    /// <remarks>
    ///   <para>This property is applicable when <see cref="Category"/> is set to
    ///   <see cref="OrganisationCategory.Establishment"/>.</para>
    /// </remarks>
    /// <seealso cref="EstablishmentNumber"/>
    public EstablishmentType? EstablishmentType { get; set; }

    /// <summary>
    /// The establishment number of the organisation.
    /// </summary>
    /// <seealso cref="EstablishmentType"/>
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// The URN of the organisation.
    /// </summary>
    public string? Urn { get; set; }

    /// <summary>
    /// The UID of the organisation.
    /// </summary>
    public string? Uid { get; set; }

    /// <summary>
    /// The UPIN of the organisation.
    /// </summary>
    public string? Upin { get; set; }

    /// <summary>
    /// The UKPRN of the organisation.
    /// </summary>
    public string? Ukprn { get; set; }

    /// <summary>
    /// Indicates when the organisation was closed.
    /// </summary>
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// The address of the organisation.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The source system of the organisation.
    /// </summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// The provider type name of the organisation.
    /// </summary>
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// The provider type code of the organisation.
    /// </summary>
    public int? ProviderTypeCode { get; set; }

    /// <summary>
    /// The GIAS provider type of the organisation.
    /// </summary>
    public string? GiasProviderType { get; set; }

    /// <summary>
    /// The PIMS provider type of the organisation.
    /// </summary>
    public string? PimsProviderType { get; set; }

    /// <summary>
    /// The PIMS provider type code of the organisation.
    /// </summary>
    public int? PimsProviderTypeCode { get; set; }

    /// <summary>
    /// The PIMS status name of the organisation.
    /// </summary>
    public string? PimsStatusName { get; set; }

    /// <summary>
    /// The PIMS status of the organisation.
    /// </summary>
    public int? PimsStatus { get; set; }

    /// <summary>
    /// The GIAS status name of the organisation.
    /// </summary>
    public string? GiasStatusName { get; set; }

    /// <summary>
    /// The GIAS status of the organisation.
    /// </summary>
    public int? GiasStatus { get; set; }

    /// <summary>
    /// The master provider status name of the organisation.
    /// </summary>
    public string? MasterProviderStatusName { get; set; }

    /// <summary>
    /// The master provider status code of the organisation.
    /// </summary>
    public int? MasterProviderStatusCode { get; set; }

    /// <summary>
    /// Indicates when the organisation was opened.
    /// </summary>
    public string? OpenedOn { get; set; }

    /// <summary>
    /// The district administrative name of the organisation.
    /// </summary>
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// The district administrative code of the organisation.
    /// </summary>
    public string? DistrictAdministrativeCode { get; set; }

    /// <summary>
    /// Indicates if the organisation is on APAR.
    /// </summary>
    public string? IsOnApar { get; set; }
}
