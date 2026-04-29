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
    public required Guid ApplicationId { get; set; }

    /// <summary>
    /// The page number to retrieve (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// The number of records per page.
    /// </summary>
    public required int PageSize { get; init; }

    ///// <summary>
    ///// The user status to filter by (e.g., "0" or "1").
    ///// </summary>
    //public string? Status { get; init; }

    ///// <summary>
    ///// The start date for the user retrieval range.
    ///// </summary>
    //public DateTime? DateFrom { get; init; }

    ///// <summary>
    ///// The end date for the user retrieval range.
    ///// </summary>
    //public DateTime? DateTo { get; init; }
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

    [JsonPropertyName("organisation_id")]
    public required Guid OrganisationId { get; init; }

    // --- Added fields to match JSON contract ---
    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    [JsonPropertyName("user_status")]
    public int? UserStatus { get; init; }

    [JsonPropertyName("user_createdAt")]
    public DateTime? UserCreatedAt { get; init; }

    [JsonPropertyName("user_updatedAt")]
    public DateTime? UserUpdatedAt { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// The name of the organisation.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("regionCode")]
    public string? RegionCode { get; set; }

    /// <summary>
    /// The category of the organisation.
    /// </summary>
    [JsonPropertyName("Category")]
    public string? Category { get; init; }

    [JsonPropertyName("DistrictAdministrative_name")]
    public string? DistrictAdministrativeNameAlt { get; init; }

    [JsonPropertyName("masteringCode")]
    public string? MasteringCode { get; init; }

    [JsonPropertyName("ProviderProfileID")]
    public string? ProviderProfileID { get; init; }

    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; init; }

    [JsonPropertyName("UPIN")]
    public string? Upin { get; init; }

    [JsonPropertyName("ProviderTypeName")]
    public string? ProviderTypeName { get; init; }

    [JsonPropertyName("GIASProviderType")]
    public string? GiasProviderType { get; init; }

    [JsonPropertyName("PIMSProviderType")]
    public string? PimsProviderType { get; init; }

    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; init; }

    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PimsProviderTypeCode { get; init; }

    [JsonPropertyName("PIMSStatus")]
    public string? PimsStatus { get; init; }

    [JsonPropertyName("OpenedOn")]
    public string? OpenedOn { get; init; }

    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; init; }

    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; init; }

    [JsonPropertyName("DistrictAdministrative_code")]
    public string? DistrictAdministrativeCodeAlt { get; init; }

    [JsonPropertyName("PIMSStatusName")]
    public string? PimsStatusName { get; init; }

    [JsonPropertyName("GIASStatus")]
    public int? GiasStatus { get; init; }

    [JsonPropertyName("GIASStatusName")]
    public string? GiasStatusName { get; init; }

    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; init; }

    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; init; }

    [JsonPropertyName("LegalName")]
    public string? LegalName { get; init; }

    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnAPAR { get; init; }

    [JsonPropertyName("localAuthorityId")]
    public Guid? LocalAuthorityId { get; init; }

    [JsonPropertyName("localAuthorityCode")]
    public string? LocalAuthorityCode { get; init; }

    [JsonPropertyName("localAuthorityName")]
    public string? LocalAuthorityName { get; init; }

    /// <summary>
    /// The type of the organisation.
    /// </summary>
    [JsonPropertyName("Type")]
    public string? Type { get; init; }

    /// <summary>
    /// The URN of the organisation.
    /// </summary>
    [JsonPropertyName("URN")]
    public string? Urn { get; init; }

    /// <summary>
    /// The UID of the organisation.
    /// </summary>
    [JsonPropertyName("UID")]
    public string? Uid { get; init; }

    /// <summary>
    /// The UKPRN of the organisation.
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
