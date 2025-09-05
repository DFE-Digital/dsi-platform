using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

internal sealed record OrganisationByIdDto : OrganisationDto
{
    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("type")]
    public string? EstablishmentType { get; init; }

    public Organisation MapToOrganisation()
    {
        return this.MapToOrganisation(
            EnumHelpers.MapEnum<OrganisationStatus>(this.Status),
            this.Category,
            this.EstablishmentType
        );
    }
}
