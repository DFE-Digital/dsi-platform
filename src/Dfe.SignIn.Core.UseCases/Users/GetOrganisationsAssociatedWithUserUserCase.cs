using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting all of the organisations that are associated with a particular user.
/// </summary>
public sealed class GetOrganisationsAssociatedWithUserUseCase(
    DbOrganisationsContext unitOfWork
)
    : Interactor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetOrganisationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var organisations = await unitOfWork.UserOrganisations
            .Where(x => x.UserId == context.Request.UserId)
            .Select(x => OrganisationHelpers.OrganisationFromEntity(x.Organisation))
            .ToArrayAsync(cancellationToken);

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
