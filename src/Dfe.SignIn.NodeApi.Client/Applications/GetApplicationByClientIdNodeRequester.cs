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
public sealed class GetApplicationByClientIdNodeRequester(
    [FromKeyedServices(NodeApiName.Applications)] HttpClient httpClient
) : Interactor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationByClientIdResponse> InvokeAsync(
        InteractionContext<GetApplicationByClientIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await httpClient.GetFromJsonOrDefaultAsync<ApplicationDto>(
            $"services/{context.Request.ClientId}",
            cancellationToken
        );

        Uri? serviceHomeUrl = !string.IsNullOrWhiteSpace(response?.RelyingParty.ServiceHome)
            ? new Uri(response.RelyingParty.ServiceHome)
            : null;

        return new GetApplicationByClientIdResponse {
            Application = response is null ? null : new() {
                ApiSecret = response.RelyingParty.ApiSecret,
                ClientId = response.RelyingParty.ClientId,
                Description = response.Description,
                Id = response.Id,
                Name = response.Name,
                ServiceHomeUrl = serviceHomeUrl,
                IsExternalService = response.IsExternalService,
                IsHiddenService = response.IsHiddenService,
                IsIdOnlyService = response.IsIdOnlyService
            }
        };
    }
}
