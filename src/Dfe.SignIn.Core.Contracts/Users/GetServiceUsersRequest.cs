using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request for retrieving service users with pagination and optional filters.
/// </summary>
[AssociatedResponse(typeof(GetServiceUsersResponse))]
public sealed record GetServiceUsersRequest
{
    /// <summary>
    /// The unique client ID of the service application.
    /// </summary>
    public string ClientId { get; init; }

    /// <summary>
    /// The unique identifier of the application for which to retrieve service users.
    /// </summary>
    public required Guid ApplicationId { get; set; }

    ///// <summary>
    ///// The page number to retrieve (1-based).
    ///// </summary>
    //public required int PageNumber { get; init; }

    ///// <summary>
    ///// The number of records per page.
    ///// </summary>
    //public required int PageSize { get; init; }

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
    public required Guid Id { get; init; }

    /// <summary>
    /// The name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The category of the organisation.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// The type of the organisation.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The URN of the organisation.
    /// </summary>
    public string? Urn { get; init; }

    /// <summary>
    /// The UID of the organisation.
    /// </summary>
    public string? Uid { get; init; }

    /// <summary>
    /// The UKPRN of the organisation.
    /// </summary>
    public string? Ukprn { get; init; }

    /// <summary>
    /// The establishment number of the organisation.
    /// </summary>
    public string? EstablishmentNumber { get; init; }

    /// <summary>
    /// The status of the organisation.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// The date the organisation was closed.
    /// </summary>
    public DateOnly? ClosedOn { get; init; }

    /// <summary>
    /// The address of the organisation.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// The telephone number of the organisation.
    /// </summary>
    public string? Telephone { get; init; }

    /// <summary>
    /// The statutory low age for the organisation.
    /// </summary>
    public int? StatutoryLowAge { get; init; }

    /// <summary>
    /// The statutory high age for the organisation.
    /// </summary>
    public int? StatutoryHighAge { get; init; }

    /// <summary>
    /// The legacy ID of the organisation.
    /// </summary>
    public long? LegacyId { get; init; }

    /// <summary>
    /// The company registration number of the organisation.
    /// </summary>
    public string? CompanyRegistrationNumber { get; init; }

    /// <summary>
    /// The phase of education for the organisation.
    /// </summary>
    public int? PhaseOfEducation { get; init; }

    /// <summary>
    /// The provider profile ID.
    /// </summary>
    public string? ProviderProfileId { get; init; }

    /// <summary>
    /// The source system of the organisation data.
    /// </summary>
    public string? SourceSystem { get; init; }
}

/// <summary>
/// Represents a role associated with a service user.
/// </summary>
public sealed record ServiceUserRoleDto
{
    /// <summary>
    /// The unique identifier of the role.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the role.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The code of the role.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The numeric ID of the role.
    /// </summary>
    public required string NumericId { get; init; }

    /// <summary>
    /// The status of the role.
    /// </summary>
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
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The user's family (last) name.
    /// </summary>
    public string? FamilyName { get; init; }

    /// <summary>
    /// The user's given (first) name.
    /// </summary>
    public string? GivenName { get; init; }

    /// <summary>
    /// The user's status (e.g., active, inactive).
    /// </summary>
    public string? UserStatus { get; init; }

    /// <summary>
    /// The organisation associated with the user.
    /// </summary>
    public ServiceUserOrganisationDto? Organisation { get; init; }

    /// <summary>
    /// The name of the user's role.
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    /// The identifier of the user's role.
    /// </summary>
    public string? RoleId { get; init; }

    /// <summary>
    /// The date and time the user was approved (ISO 8601 format).
    /// </summary>
    public DateTime? ApprovedAt { get; init; }

    /// <summary>
    /// The date and time the user was last updated (ISO 8601 format).
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Service roles for the user.
    /// </summary>
    public IReadOnlyList<ServiceUserRoleDto>? Roles { get; init; }
}

/// <summary>
/// Response containing a paginated list of service users.
/// </summary>
public sealed record GetServiceUsersResponse
{
    /// <summary>
    /// The list of service users for the requested page.
    /// </summary>
    public required IReadOnlyList<ServiceUserDto> Users { get; init; }

    /// <summary>
    /// The total number of records available.
    /// </summary>
    public required int NumberOfRecords { get; init; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// The total number of pages available.
    /// </summary>
    public required int NumberOfPages { get; init; }
}
