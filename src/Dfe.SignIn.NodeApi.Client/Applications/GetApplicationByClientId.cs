using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Applications.Interactions;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Applications;

/// <summary>
/// ApiRequester for obtaining an application.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationByClientId_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Applications)] HttpClient httpClient
) : Interactor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationByClientIdResponse> InvokeAsync(
        InteractionContext<GetApplicationByClientIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await httpClient.GetFromJsonOrDefaultAsync<ApplicationModelDto>(
            $"services/{context.Request.ClientId}",
            cancellationToken
        );

        return new GetApplicationByClientIdResponse {
            Application = response is null ? null : new() {
                ApiSecret = response.RelyingParty.ApiSecret,
                ClientId = response.RelyingParty.ClientId,
                Description = response.Description,
                Id = response.Id,
                Name = response.Name,
                ServiceHomeUrl = new Uri(response.RelyingParty.ServiceHome),
                IsExternalService = response.IsExternalService,
                IsHiddenService = response.IsHiddenService,
                IsIdOnlyService = response.IsIdOnlyService
            }
        };
    }
}
