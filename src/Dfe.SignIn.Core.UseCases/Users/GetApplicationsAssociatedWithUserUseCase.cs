using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor for retrieving applications associated with a user.
/// </summary>
/// <param name="uowOrganisations">Unit of work for accessing organisation-related data.</param>
public sealed class GetApplicationsAssociatedWithUserUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse>
{
    public override async Task<GetApplicationsAssociatedWithUserResponse> InvokeAsync(
     InteractionContext<GetApplicationsAssociatedWithUserRequest> context,
     CancellationToken cancellationToken)
    {
        context.ThrowIfHasValidationErrors();

        var serviceMappings = await uowOrganisations.Repository<UserServiceEntity>()
            .Where(x => x.UserId == context.Request.UserId && x.Status == 1) // status 1 = Active access
            .Where(x => x.OrganisationId != null && x.ServiceId != null)
            .Select(x => new UserApplicationMapping {
                UserId = x.UserId,
                AccessGranted = x.CreatedAt,
                OrganisationId = x.OrganisationId!.Value,
                ApplicationId = x.ServiceId!.Value
            })
            .ToListAsync(cancellationToken);

        return new GetApplicationsAssociatedWithUserResponse {
            UserApplicationMappings = serviceMappings
        };
    }
}
