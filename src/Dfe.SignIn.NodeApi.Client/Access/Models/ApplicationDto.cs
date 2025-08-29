using Dfe.SignIn.Core.InternalModels.Access;

namespace Dfe.SignIn.NodeApi.Client.Access.Models;

internal sealed record ApplicationDto
{
    public required Guid UserId { get; init; }
    public required Guid ServiceId { get; init; }
    public required Guid OrganisationId { get; init; }
    public required RoleDto[] Roles { get; init; } = [];
    public required IdentifiersDto[] Identifiers { get; init; } = [];
    public required DateTime AccessGrantedOn { get; init; }
}

internal sealed record RoleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required long NumericId { get; init; }
}

internal sealed record StatusDto
{
    public required RoleStatus Id { get; init; }
}

internal sealed record IdentifiersDto
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}
