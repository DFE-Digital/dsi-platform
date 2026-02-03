using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Applications;

/// <summary>
/// ApiRequester for obtaining an application.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationApiConfigurationNodeRequester(
    IInteractionDispatcher interaction,
    [FromKeyedServices(NodeApiName.Applications)] HttpClient applicationsClient
) : Interactor<GetApplicationApiConfigurationRequest, GetApplicationApiConfigurationResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationApiConfigurationResponse> InvokeAsync(
        InteractionContext<GetApplicationApiConfigurationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await applicationsClient.GetFromJsonOrDefaultAsync<ApplicationDto>(
            $"services/{context.Request.ClientId}",
            cancellationToken
        ) ?? throw new ApplicationNotFoundException(null, context.Request.ClientId);

        var apiSecretResponse = await interaction.DispatchAsync(
            new DecryptPublicApiSecretRequest {
                EncryptedApiSecret = response.RelyingParty.ApiSecret ?? string.Empty
            }
        ).To<DecryptedPublicApiSecretResponse>();

        return new GetApplicationApiConfigurationResponse {
            Configuration = new() {
                ClientId = response.RelyingParty.ClientId,
                ApiSecret = apiSecretResponse.ApiSecret
            }
        };
    }
}
