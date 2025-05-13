using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client.Users;

/// <summary>
/// Represents the body of a request to query a specific organisation of a user.
/// </summary>
public record QueryUserOrganisation_PublicApiRequestBody()
{
    /// <summary>
    /// Specifies the organisation filtering requirements.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();

    /// <summary>
    /// Specifies the level of organisation detail required in the callback response.
    /// </summary>
    [EnumDataType(typeof(OrganisationDetailLevel))]
    public OrganisationDetailLevel DetailLevel { get; init; } = OrganisationDetailLevel.Basic;
}

/// <summary>
/// Represents a request to query a specific organisation of a user.
/// </summary>
public sealed record QueryUserOrganisation_PublicApiRequest() : QueryUserOrganisation_PublicApiRequestBody
{
    /// <summary>
    /// Specifies the unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Specifies the unique DfE Sign-in ID of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response to request to <see cref="QueryUserOrganisation_PublicApiRequest"/>.
/// </summary>
public sealed record QueryUserOrganisation_PublicApiResponse()
{
    /// <summary>
    /// Gets the unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the detail level of the organisation.
    /// </summary>
    [EnumDataType(typeof(OrganisationDetailLevel))]
    public required OrganisationDetailLevel DetailLevel { get; init; }

    /// <summary>
    /// Gets organisation details when query criteria was met; otherwise, a value of
    /// null indicating that the organisation was not matched.
    /// </summary>
    public required OrganisationDetails? Organisation { get; init; }
}

internal sealed class QueryUserOrganisation_PublicApiRequester
    : PublicApiPostRequester<QueryUserOrganisation_PublicApiRequest, QueryUserOrganisation_PublicApiResponse>
{
    public QueryUserOrganisation_PublicApiRequester(
        IPublicApiClient client, JsonSerializerOptions jsonOptions, string endpoint
    ) : base(client, jsonOptions, endpoint) { }

    /// <inheritdoc/>
    protected override string TransformEndpoint(QueryUserOrganisation_PublicApiRequest request, string endpoint)
    {
        return endpoint
            .Replace("{userId}", request.UserId.ToString())
            .Replace("{organisationId}", request.OrganisationId.ToString());
    }
}
