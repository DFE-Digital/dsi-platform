using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request for retrieving service users with pagination and optional filters.
/// </summary>
[AssociatedResponse(typeof(GetServiceUsersResponse))]
public sealed record GetServiceUsersRequest
{
    /// <summary>
    /// The unique identifier of the application for which to retrieve service users.
    /// </summary>
    public required Guid ApplicationId { get; init; }

    /// <summary>
    /// The page number to retrieve (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// The number of records per page.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Optional filter by user status (0 = inactive, 1 = active).
    /// </summary>
    public int? UserStatus { get; init; }

    /// <summary>
    /// Optional start of date range filter (inclusive), applied against CreatedAt.
    /// </summary>
    public DateTimeOffset? DateFrom { get; init; }

    /// <summary>
    /// Optional end of date range filter (inclusive), applied against CreatedAt.
    /// </summary>
    public DateTimeOffset? DateTo { get; init; }
}

/// <summary>
/// Represents an organisation associated with a service user.
/// </summary>
public sealed record ServiceUserOrganisationDto
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    /// <summary>
    /// The unique identifier of the organisation (alternate property).
    /// </summary>
    [JsonPropertyName("organisation_id")]
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// The unique identifier of the user associated with the organisation.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    /// <summary>
    /// The status of the user associated with the organisation.
    /// </summary>
    [JsonPropertyName("user_status")]
    public int? UserStatus { get; init; }

    /// <summary>
    /// The date the user was created.
    /// </summary>
    [JsonPropertyName("user_createdAt")]
    public DateTime? UserCreatedAt { get; init; }

    /// <summary>
    /// The date the user was last updated.
    /// </summary>
    [JsonPropertyName("user_updatedAt")]
    public DateTime? UserUpdatedAt { get; init; }

    /// <summary>
    /// The date the organisation record was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; init; }

    /// <summary>
    /// The date the organisation record was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// The name of the organisation.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The region code of the organisation.
    /// </summary>
    [JsonPropertyName("regionCode")]
    public string? RegionCode { get; set; }

    /// <summary>
    /// The category of the organisation.
    /// </summary>
    [JsonPropertyName("Category")]
    public string? Category { get; init; }

    /// <summary>
    /// The alternate district administrative name.
    /// </summary>
    [JsonPropertyName("DistrictAdministrative_name")]
    public string? DistrictAdministrativeNameAlt { get; init; }

    /// <summary>
    /// The mastering code of the organisation.
    /// </summary>
    [JsonPropertyName("masteringCode")]
    public string? MasteringCode { get; init; }

    /// <summary>
    /// The provider profile ID.
    /// </summary>
    [JsonPropertyName("ProviderProfileID")]
    public string? ProviderProfileID { get; init; }

    /// <summary>
    /// The source system of the organisation data.
    /// </summary>
    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; init; }

    /// <summary>
    /// The UPIN (Unique Provider Identification Number).
    /// </summary>
    [JsonPropertyName("UPIN")]
    public string? Upin { get; init; }

    /// <summary>
    /// The provider type name from GIAS.
    /// </summary>
    [JsonPropertyName("ProviderTypeName")]
    public string? ProviderTypeName { get; init; }

    /// <summary>
    /// The provider type from GIAS.
    /// </summary>
    [JsonPropertyName("GIASProviderType")]
    public string? GiasProviderType { get; init; }

    /// <summary>
    /// The provider type from PIMS.
    /// </summary>
    [JsonPropertyName("PIMSProviderType")]
    public string? PimsProviderType { get; init; }

    /// <summary>
    /// The provider type code.
    /// </summary>
    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; init; }

    /// <summary>
    /// The provider type code from PIMS.
    /// </summary>
    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PimsProviderTypeCode { get; init; }

    /// <summary>
    /// The status from PIMS.
    /// </summary>
    [JsonPropertyName("PIMSStatus")]
    public string? PimsStatus { get; init; }

    /// <summary>
    /// The date the organisation was opened.
    /// </summary>
    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; init; }

    /// <summary>
    /// The district administrative name.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; init; }

    /// <summary>
    /// The district administrative code.
    /// </summary>
    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; init; }

    /// <summary>
    /// The alternate district administrative code.
    /// </summary>
    [JsonPropertyName("DistrictAdministrative_code")]
    public string? DistrictAdministrativeCodeAlt { get; init; }

    /// <summary>
    /// The status name from PIMS.
    /// </summary>
    [JsonPropertyName("PIMSStatusName")]
    public string? PimsStatusName { get; init; }

    /// <summary>
    /// The status from GIAS.
    /// </summary>
    [JsonPropertyName("GIASStatus")]
    public int? GiasStatus { get; init; }

    /// <summary>
    /// The status name from GIAS.
    /// </summary>
    [JsonPropertyName("GIASStatusName")]
    public string? GiasStatusName { get; init; }

    /// <summary>
    /// The master provider status code.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; init; }

    /// <summary>
    /// The master provider status name.
    /// </summary>
    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; init; }

    /// <summary>
    /// The legal name of the organisation.
    /// </summary>
    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    /// <summary>
    /// Indicates if the organisation is on APAR.
    /// </summary>
    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnAPAR { get; init; }

    /// <summary>
    /// The local authority ID.
    /// </summary>
    [JsonPropertyName("localAuthorityId")]
    public Guid? LocalAuthorityId { get; init; }

    /// <summary>
    /// The local authority code.
    /// </summary>
    [JsonPropertyName("localAuthorityCode")]
    public string? LocalAuthorityCode { get; init; }

    /// <summary>
    /// The local authority name.
    /// </summary>
    [JsonPropertyName("localAuthorityName")]
    public string? LocalAuthorityName { get; init; }

    /// <summary>
    /// The type of the organisation.
    /// </summary>
    [JsonPropertyName("Type")]
    public string? Type { get; init; }

    /// <summary>
    /// The URN (Unique Reference Number) of the organisation.
    /// </summary>
    [JsonPropertyName("URN")]
    public string? Urn { get; init; }

    /// <summary>
    /// The UID (Unique Identifier) of the organisation.
    /// </summary>
    [JsonPropertyName("UID")]
    public string? Uid { get; init; }

    /// <summary>
    /// The UKPRN (UK Provider Reference Number) of the organisation.
    /// </summary>
    [JsonPropertyName("UKPRN")]
    public string? Ukprn { get; init; }

    /// <summary>
    /// The establishment number of the organisation.
    /// </summary>
    [JsonPropertyName("EstablishmentNumber")]
    public string? EstablishmentNumber { get; init; }

    /// <summary>
    /// The status of the organisation.
    /// </summary>
    [JsonPropertyName("Status")]
    public int Status { get; init; }

    /// <summary>
    /// The date the organisation was closed.
    /// </summary>
    [JsonPropertyName("ClosedOn")]
    public DateOnly? ClosedOn { get; init; }

    /// <summary>
    /// The address of the organisation.
    /// </summary>
    [JsonPropertyName("Address")]
    public string? Address { get; init; }

    /// <summary>
    /// The telephone number of the organisation.
    /// </summary>
    [JsonPropertyName("telephone")]
    public string? Telephone { get; init; }

    /// <summary>
    /// The statutory low age for the organisation.
    /// </summary>
    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; init; }

    /// <summary>
    /// The statutory high age for the organisation.
    /// </summary>
    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; init; }

    /// <summary>
    /// The legacy ID of the organisation.
    /// </summary>
    [JsonPropertyName("legacyId")]
    public string? LegacyId { get; init; }

    /// <summary>
    /// The company registration number of the organisation.
    /// </summary>
    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; init; }

    /// <summary>
    /// The phase of education for the organisation.
    /// </summary>
    [JsonPropertyName("phaseOfEducation")]
    public int? PhaseOfEducation { get; init; }
}

/// <summary>
/// Represents a role associated with a service user.
/// </summary>
public sealed record ServiceUserRoleDto
{
    /// <summary>
    /// The unique identifier of the role.
    /// </summary>
    /// <summary>
    /// The unique identifier of the role.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The name of the role.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The code of the role.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// The numeric ID of the role.
    /// </summary>
    [JsonPropertyName("numericId")]
    public required string NumericId { get; init; }

    /// <summary>
    /// The status of the role.
    /// </summary>
    [JsonPropertyName("status")]
    public required int Status { get; init; }
}

/// <summary>
/// Represents a service user in the response.
/// </summary>
public sealed record ServiceUserDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>
    /// The user's family (last) name.
    /// </summary>
    [JsonPropertyName("familyName")]
    public string? FamilyName { get; init; }

    /// <summary>
    /// The user's given (first) name.
    /// </summary>
    [JsonPropertyName("givenName")]
    public string? GivenName { get; init; }

    /// <summary>
    /// The user's status (e.g., active, inactive).
    /// </summary>
    [JsonPropertyName("userStatus")]
    public int? UserStatus { get; init; }

    /// <summary>
    /// The organisation associated with the user.
    /// </summary>
    [JsonPropertyName("organisation")]
    public ServiceUserOrganisationDto? Organisation { get; init; }

    /// <summary>
    /// The name of the user's role.
    /// </summary>
    [JsonPropertyName("roleName")]
    public string? RoleName { get; init; }

    /// <summary>
    /// The identifier of the user's role.
    /// </summary>
    [JsonPropertyName("roleId")]
    public short? RoleId { get; init; }

    /// <summary>
    /// The date and time the user was approved (ISO 8601 format).
    /// </summary>
    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; init; }

    /// <summary>
    /// The date and time the user was last updated (ISO 8601 format).
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Service roles for the user.
    /// </summary>
    [JsonPropertyName("roles")]
    public IReadOnlyList<ServiceUserRoleDto>? Roles { get; init; }
}

/// <summary>
/// Response containing a paginated list of service users.
/// </summary>
public sealed record GetServiceUsersResponse()
{
    /// <summary>
    /// The list of service users for the requested page.
    /// </summary>
    [JsonPropertyName("users")]
    public required IReadOnlyList<ServiceUserDto> Users { get; init; }

    /// <summary>
    /// The total number of records available.
    /// </summary>
    [JsonPropertyName("numberOfRecords")]
    public required int NumberOfRecords { get; init; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    [JsonPropertyName("page")]
    public required int Page { get; init; }

    /// <summary>
    /// The total number of pages available.
    /// </summary>
    [JsonPropertyName("numberOfPages")]
    public required int NumberOfPages { get; init; }

    /// <summary>
    /// The date range applied to the query, if applicable (e.g., "2024-01-01 to 2024-01-31"), used when the response is filtered by a date range.
    /// This provides clarity on the actual date range used in the query, especially if it was truncated due to exceeding maximum allowed range.
    /// </summary>
    [JsonPropertyName("dateRange")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateRange { get; init; }

    /// <summary>
    /// The warning message to include in the response, if applicable (e.g., when the date range is truncated due to exceeding maximum allowed range).
    /// </summary>
    [JsonPropertyName("warning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Warning { get; init; }

    /// <summary>
    /// Returns an empty response for a given page number, used when there are no records to return.
    /// </summary>
    /// <param name="pageNumber">The page number for which the empty response is generated.</param>
    /// <returns>An empty <see cref="GetServiceUsersResponse"/> instance.</returns>
    public static GetServiceUsersResponse Empty(int pageNumber) => new() {
        Users = [],
        NumberOfRecords = 0,
        Page = pageNumber,
        NumberOfPages = 0
    };

    /// <summary>
    /// Returns a response containing the provided list of service users and pagination details.
    /// </summary>
    /// <param name="users">The list of service users.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>A <see cref="GetServiceUsersResponse"/> instance containing the provided users and pagination details.</returns>
    public static GetServiceUsersResponse FromUsers(
        IReadOnlyList<ServiceUserDto> users,
        int totalRecords,
        int pageNumber,
        int pageSize) => new() {
            Users = users,
            NumberOfRecords = totalRecords,
            Page = pageNumber,
            NumberOfPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
}
