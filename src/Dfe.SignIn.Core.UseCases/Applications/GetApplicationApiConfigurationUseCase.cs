using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Applications;

/// <summary>
/// Use case responsible for obtaining the Public API configuration of an application.
/// </summary>
public sealed class GetApplicationApiConfigurationUseCase(
    IInteractionDispatcher interaction,
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetApplicationApiConfigurationRequest, GetApplicationApiConfigurationResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationApiConfigurationResponse> InvokeAsync(
        InteractionContext<GetApplicationApiConfigurationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var serviceEntity = await uowOrganisations.Repository<ServiceEntity>()
            .SingleOrDefaultAsync(
                entity => entity.ClientId == context.Request.ClientId,
                cancellationToken
            ) ?? throw new ApplicationNotFoundException(null, context.Request.ClientId);

        var apiSecretResponse = await interaction.DispatchAsync(
            new DecryptApiSecretRequest {
                EncryptedApiSecret = serviceEntity.ApiSecret ?? string.Empty
            }
        ).To<DecryptApiSecretResponse>();

        return new GetApplicationApiConfigurationResponse {
            Configuration = new() {
                ClientId = serviceEntity.ClientId,
                ApiSecret = apiSecretResponse.ApiSecret
            }
        };
    }
}
