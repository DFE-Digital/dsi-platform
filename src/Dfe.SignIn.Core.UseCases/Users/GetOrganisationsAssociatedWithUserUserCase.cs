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
    DbOrganisationsContext organisationDbContext
)
    : Interactor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetOrganisationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();
        var organisations2 = organisationDbContext.UserOrganisations.ToList()
            .Where(x => x.UserId == context.Request.UserId);

        var organisations = organisationDbContext.UserOrganisations
            .Where(x => x.UserId == context.Request.UserId)
            .Include(x => x.Organisation)
                .ThenInclude(x => x.Associations)
                .ThenInclude(x => x.AssociatedOrganisation)
            .ToList();

        var x = organisations.Select(x => OrganisationHelpers.OrganisationFromEntity(x.Organisation));

        return Task.FromResult(new GetOrganisationsAssociatedWithUserResponse {
            Organisations = x
        });
    }
}
