using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Search;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.NodeApi.Client.Search;

/// <summary>
/// An interactor to update the search index for a given user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Search)]
public sealed class UpdateUserInSearchIndexNodeRequester(
    [FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<UpdateUserInSearchIndexNodeRequester> logger
) : Interactor<UpdateUserInSearchIndexRequest, UpdateUserInSearchIndexResponse>
{
    /// <inheritdoc/>
    public override async Task<UpdateUserInSearchIndexResponse> InvokeAsync(
        InteractionContext<UpdateUserInSearchIndexRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        try {
            var response = await searchClient.PostAsJsonAsync($"users/update-index", new UpdateUserInSearchIndexRequestDto {
                Id = context.Request.UserId,
            }, cancellationToken: cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "Updated search index for user '{UserId}'.",
                context.Request.UserId
            );
        }
        catch (Exception ex) {
            logger.LogError(
                ex,
                "Unable to update search index for user '{UserId}'.",
                context.Request.UserId
            );
        }

        return new UpdateUserInSearchIndexResponse { };
    }
}
