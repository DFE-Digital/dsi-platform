using System.Text.Json;
using Dfe.SignIn.PublicApi.Contracts.Users;

namespace Dfe.SignIn.PublicApi.Client.Users;

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
