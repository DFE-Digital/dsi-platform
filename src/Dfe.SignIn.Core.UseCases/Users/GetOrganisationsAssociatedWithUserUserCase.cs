using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Dfe.SignIn.Core.UseCases.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting all of the organisations that are associated with a particular user.
/// </summary>
public sealed class GetOrganisationsAssociatedWithUserUseCase(
    IUnitOfWorkOrganisations unitOfWork
)
    : Interactor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetOrganisationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var organisations = await unitOfWork.Repository<UserOrganisationEntity>()
            .Where(x => x.UserId == context.Request.UserId)
            .Select(x => OrganisationHelpers.OrganisationFromEntity(x.Organisation))
            .ToArrayAsync(cancellationToken);

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
