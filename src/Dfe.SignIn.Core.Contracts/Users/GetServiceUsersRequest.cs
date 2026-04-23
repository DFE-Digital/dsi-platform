namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request for retrieving service users with pagination.
/// </summary>
public sealed class GetServiceUsersRequest
{
    /// <summary>
    /// The page number to retrieve (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// The number of records per page.
    /// </summary>
    public required int PageSize { get; init; }
}

/// <summary>
/// Represents a service user in the response.
/// </summary>
public sealed class ServiceUserDto
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
    public string? Organisation { get; init; }

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
    /// Service roles for the user. Not yet populated in .NET implementation.
    /// </summary>
    public IReadOnlyList<object>? Roles { get; init; } = []; // TODO: Populate with actual role data
}

/// <summary>
/// Response containing a paginated list of service users.
/// </summary>
public sealed class GetServiceUsersResponse
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
