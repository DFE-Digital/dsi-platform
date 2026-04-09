using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Use case for retrieving the legacy numeric and text identifiers stored on the
/// user_organisation record. These identifiers are used by relying parties for
/// legacy system integration.
/// </summary>
public sealed class GetUserOrganisationIdentifiersUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetUserOrganisationIdentifiersRequest, GetUserOrganisationIdentifiersResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserOrganisationIdentifiersResponse> InvokeAsync(
        InteractionContext<GetUserOrganisationIdentifiersRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userOrganisation = await uowOrganisations
            .Repository<UserOrganisationEntity>()
            .Where(x => x.UserId == context.Request.UserId
                     && x.OrganisationId == context.Request.OrganisationId)
            .FirstOrDefaultAsync(cancellationToken);

        return new GetUserOrganisationIdentifiersResponse {
            NumericIdentifier = userOrganisation?.NumericIdentifier,
            TextIdentifier = userOrganisation?.TextIdentifier,
        };
    }
}
