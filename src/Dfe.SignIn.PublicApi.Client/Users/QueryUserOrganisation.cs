using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client.Users;

/// <summary>
/// Represents the body of a request to query a specific organisation of a user.
/// </summary>
public record QueryUserOrganisationApiRequestBody()
{
    /// <summary>
    /// Specifies the organisation filtering requirements.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();
}

/// <summary>
/// Represents a request to query a specific organisation of a user.
/// </summary>
public sealed record QueryUserOrganisationApiRequest() : QueryUserOrganisationApiRequestBody
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
/// Response to request to <see cref="QueryUserOrganisationApiRequest"/>.
/// </summary>
public sealed record QueryUserOrganisationApiResponse()
{
    /// <summary>
    /// Gets the unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets organisation details when query criteria was met; otherwise, a value of
    /// null indicating that the organisation was not matched.
    /// </summary>
    public required OrganisationDetails? Organisation { get; init; }
}

internal sealed class QueryUserOrganisationApiRequester
    : PublicApiPostRequester<QueryUserOrganisationApiRequest, QueryUserOrganisationApiResponse>
{
    public QueryUserOrganisationApiRequester(
        IPublicApiClient client, JsonSerializerOptions jsonOptions, string endpoint
    ) : base(client, jsonOptions, endpoint) { }

    /// <inheritdoc/>
    protected override string TransformEndpoint(QueryUserOrganisationApiRequest request, string endpoint)
    {
        return endpoint
            .Replace("{userId}", request.UserId.ToString())
            .Replace("{organisationId}", request.OrganisationId.ToString());
    }
}
