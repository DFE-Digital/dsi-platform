using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Applications;

/// <summary>
/// Use case responsible for obtaining information about an application.
/// </summary>
public sealed class GetApplicationByClientIdUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationByClientIdResponse> InvokeAsync(
        InteractionContext<GetApplicationByClientIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var serviceEntity = await uowOrganisations.Repository<ServiceEntity>()
            .Select(x => new {
                x.Id,
                x.ClientId,
                x.Description,
                x.Name,
                x.ServiceHome,
                x.IsExternalService,
                x.IsHiddenService,
                x.IsIdOnlyService,
            })
            .SingleOrDefaultAsync(
                x => x.ClientId == context.Request.ClientId,
                cancellationToken
            ) ?? throw new ApplicationNotFoundException(null, context.Request.ClientId);

        Uri? serviceHomeUrl = !string.IsNullOrWhiteSpace(serviceEntity.ServiceHome)
            ? new Uri(serviceEntity.ServiceHome)
            : null;

        return new GetApplicationByClientIdResponse {
            Application = new() {
                Id = serviceEntity.Id,
                ClientId = serviceEntity.ClientId,
                Description = serviceEntity.Description,
                Name = serviceEntity.Name,
                ServiceHomeUrl = serviceHomeUrl,
                IsExternalService = serviceEntity.IsExternalService,
                IsHiddenService = serviceEntity.IsHiddenService,
                IsIdOnlyService = serviceEntity.IsIdOnlyService,
            }
        };
    }
}
