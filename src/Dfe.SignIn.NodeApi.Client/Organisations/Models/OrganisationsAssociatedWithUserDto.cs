using System.Text.Json.Serialization;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework.Internal;
using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

internal sealed record OrganisationsAssociatedWithUserDto
{
    public required UserOrganisationDto Organisation { get; init; }
    public RoleDto? Role { get; init; }
    public Guid[] Approvers { get; init; } = [];
    public Guid[] EndUsers { get; init; } = [];
    public string? NumericIdentifier { get; init; } = null;
    public string? TextIdentifier { get; init; } = null;
}

internal sealed record UserOrganisationDto : OrganisationDto
{
    [JsonPropertyName("status")]
    public required StatusDto Status { get; set; }

    [JsonPropertyName("category")]
    public required CategoryDto Category { get; init; }

    [JsonPropertyName("type")]
    public EstablishmentTypeDto? EstablishmentType { get; init; }

    [JsonPropertyName("role")]
    public RoleDto? Role { get; init; }

    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; set; }

    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; set; }

    [JsonPropertyName("legacyId")]
    public long? LegacyId { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; set; }

    public OrganisationModel MapToOrganisationModel()
    {
        return this.MapToOrganisationModel(
            EnumHelpers.MapEnum<OrganisationStatus>(this.Status.Id),
            this.Category.Id,
            this.EstablishmentType?.Id
        );
    }
}

internal sealed record RoleDto
{
    [JsonPropertyName("id")]

    public required int Id { get; init; }

    [JsonPropertyName("name")]

    public required string Name { get; init; }
}

internal sealed record StatusDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("tagColor")]
    public required string TagColor { get; init; }
}

internal sealed record CategoryDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

internal sealed record EstablishmentTypeDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
