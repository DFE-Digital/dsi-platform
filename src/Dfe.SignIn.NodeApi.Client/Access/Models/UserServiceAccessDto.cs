namespace Dfe.SignIn.NodeApi.Client.Access.Models;

/// <summary>
/// DTO for the response from the Access API:
/// GET /users/{userId}/services/{serviceId}/organisations/{organisationId}
/// </summary>
internal sealed record UserServiceAccessDto
{
    public required Guid UserId { get; init; }
    public required Guid ServiceId { get; init; }
    public required Guid OrganisationId { get; init; }
    public required UserServiceRoleDto[] Roles { get; init; } = [];
    public required UserServiceIdentifierDto[] Identifiers { get; init; } = [];
}

internal sealed record UserServiceRoleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required long NumericId { get; init; }
}

internal sealed record UserServiceIdentifierDto
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}
