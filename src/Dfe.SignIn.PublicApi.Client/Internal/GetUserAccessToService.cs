using System.Net.Http.Json;
using System.Text.Json;
using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.Internal;

// The following interactor is not exposed for use in applications until the
// request/response models have been properly designed.

internal sealed record GetUserAccessToServiceRequest()
{
    public required Guid UserId { get; init; }

    public required Guid OrganisationId { get; init; }
}

internal sealed record GetUserAccessToServiceResponse()
{
    public required IEnumerable<Role> Roles { get; init; }
}

internal sealed class Role()
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required string NumericId { get; init; }

    public required Status Status { get; init; }
}

internal sealed class Status()
{
    public required int Id { get; init; }
}

internal sealed class GetUserAccessToService_PublicApiRequester(
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor,
    IOptions<PublicApiOptions> optionsAccessor,
    IPublicApiClient client
) : Interactor<GetUserAccessToServiceRequest, GetUserAccessToServiceResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserAccessToServiceResponse> InvokeAsync(
        InteractionContext<GetUserAccessToServiceRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var options = optionsAccessor.Value;
        var httpClient = client.HttpClient;

        var serviceId = options.ClientId;
        var organisationId = context.Request.OrganisationId;
        var userId = context.Request.UserId;

        var httpResponse = await httpClient.GetAsync(
            $"services/{serviceId}/organisations/{organisationId}/users/{userId}",
            cancellationToken
        );
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return new GetUserAccessToServiceResponse {
                Roles = [],
            };
        }

        httpResponse.EnsureSuccessStatusCode();

        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        return await httpResponse.Content.ReadFromJsonAsync<GetUserAccessToServiceResponse>(
            jsonOptions, cancellationToken
        ) ?? throw new InvalidOperationException("Invalid response.");
    }
}
