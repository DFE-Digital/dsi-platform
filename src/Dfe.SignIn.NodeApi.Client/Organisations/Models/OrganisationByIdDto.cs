using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

internal sealed record OrganisationByIdDto : OrganisationDto
{
    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("type")]
    public string? EstablishmentType { get; init; }

    public OrganisationModel MapToOrganisationModel()
    {
        return this.MapToOrganisationModel(
            EnumHelpers.MapEnum<Core.ExternalModels.Organisations.OrganisationStatus>(this.Status),
            this.Category,
            this.EstablishmentType
        );
    }
}
