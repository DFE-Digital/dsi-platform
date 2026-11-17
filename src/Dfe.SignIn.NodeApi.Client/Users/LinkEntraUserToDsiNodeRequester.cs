using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to link an Entra user to DfE Sign-in.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class LinkEntraUserToDsiNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    ILogger<LinkEntraUserToDsiNodeRequester> logger
) : Interactor<LinkEntraUserToDsiRequest, LinkEntraUserToDsiResponse>
{
    /// <inheritdoc/>
    public override async Task<LinkEntraUserToDsiResponse> InvokeAsync(
        InteractionContext<LinkEntraUserToDsiRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpoint = $"users/{context.Request.DsiUserId}/link-entra-oid";

        var requestBody = new LinkToEntraRequestDto {
            EntraOid = context.Request.EntraUserId,
            FirstName = context.Request.FirstName,
            LastName = context.Request.LastName,
        };

        var responseMessage = await directoriesClient.PostAsJsonAsync(endpoint, requestBody, CancellationToken.None);

        if (!responseMessage.IsSuccessStatusCode) {
            // The Directories API answers 404 when it cannot link, for example because the
            // Entra identity is already linked to a different DfE Sign-In user.
            logger.LogError(
                "Directories API failed to link Entra account '{EntraUserId}' to DfE Sign-In user '{DsiUserId}'. Status: {StatusCode}.",
                context.Request.EntraUserId, context.Request.DsiUserId, (int)responseMessage.StatusCode
            );
        }

        responseMessage.EnsureSuccessStatusCode();

        var user = (await responseMessage.Content.ReadFromJsonAsync<LinkToEntraResponseDto>(CancellationToken.None))!;

        if (user.Id != context.Request.DsiUserId) {
            throw new InvalidOperationException("Response mismatch.");
        }

        return new LinkEntraUserToDsiResponse();
    }
}
