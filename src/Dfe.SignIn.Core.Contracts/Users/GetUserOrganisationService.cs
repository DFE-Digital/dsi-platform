namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a user's association with an organisation and service,
/// including identity, organisation details, and role information.
/// </summary>
public class GetUserOrganisationService
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's status identifier.
    /// </summary>
    public int UserStatus { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's family (last) name.
    /// </summary>
    public string FamilyName { get; set; }

    /// <summary>
    /// Gets or sets the user's given (first) name.
    /// </summary>
    public string GivenName { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the organisation.
    /// </summary>
    public Guid OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the organisation.
    /// </summary>
    public string OrganisationName { get; set; }

    /// <summary>
    /// Gets or sets the category identifier of the organisation.
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Unique Reference Number (URN) of the organisation.
    /// </summary>
    public string? Urn { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (UID) of the organisation.
    /// </summary>
    public string? Uid { get; set; }

    /// <summary>
    /// Gets or sets the UK Provider Reference Number (UKPRN).
    /// </summary>
    public string? Ukprn { get; set; }

    /// <summary>
    /// Gets or sets the establishment number of the organisation.
    /// </summary>
    public string? EstablishmentNumber { get; set; }

    /// <summary>
    /// Gets or sets the organisation status identifier.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the date the organisation was closed.
    /// </summary>
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// Gets or sets the address of the organisation.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the contact telephone number of the organisation.
    /// </summary>
    public string? Telephone { get; set; }

    /// <summary>
    /// Gets or sets the statutory lowest age for the organisation.
    /// </summary>
    public int? StatutoryLowAge { get; set; }

    /// <summary>
    /// Gets or sets the statutory highest age for the organisation.
    /// </summary>
    public int? StatutoryHighAge { get; set; }

    /// <summary>
    /// Gets or sets the legacy identifier for the organisation.
    /// </summary>
    public long? LegacyId { get; set; }

    /// <summary>
    /// Gets or sets the company registration number.
    /// </summary>
    public string? CompanyRegistrationNumber { get; set; }

    /// <summary>
    /// Gets or sets the provider profile identifier.
    /// </summary>
    public string? ProviderProfileID { get; set; }

    /// <summary>
    /// Gets or sets the Unique Provider Identification Number (UPIN).
    /// </summary>
    public string? UPIN { get; set; }

    /// <summary>
    /// Gets or sets the PIMS provider type.
    /// </summary>
    public string? PIMSProviderType { get; set; }

    /// <summary>
    /// Gets or sets the PIMS status.
    /// </summary>
    public string? PIMSStatus { get; set; }

    /// <summary>
    /// Gets or sets the district administrative name.
    /// </summary>
    public string? DistrictAdministrativeName { get; set; }

    /// <summary>
    /// Gets or sets the opening date of the organisation (as a string).
    /// </summary>
    public string? OpenedOn { get; set; }

    /// <summary>
    /// Gets or sets the source system from which the data originates.
    /// </summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Gets or sets the provider type name.
    /// </summary>
    public string? ProviderTypeName { get; set; }

    /// <summary>
    /// Gets or sets the GIAS provider type.
    /// </summary>
    public string? GIASProviderType { get; set; }

    /// <summary>
    /// Gets or sets the PIMS provider type code.
    /// </summary>
    public int? PIMSProviderTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the service.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the description of the service.
    /// </summary>
    public string? ServiceDescription { get; set; }

    /// <summary>
    /// Gets or sets the role name assigned to the user in the service.
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    /// Gets or sets the role code assigned to the user in the service.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the organisation role identifier.
    /// </summary>
    public short OrgRoleId { get; set; }
}
