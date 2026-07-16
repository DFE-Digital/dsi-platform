using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Organisations;

/// <summary>
/// Use case for getting information about an organisation.
/// </summary>
public sealed class GetOrganisationByIdUseCase(DbOrganisationsContext organisationsDbContext) : Interactor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{
    /// <inheritdoc/>
    public override async Task<GetOrganisationByIdResponse> InvokeAsync(
        InteractionContext<GetOrganisationByIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var organisationEntity = await organisationsDbContext.Organisations
            .Where(x => x.Id == context.Request.OrganisationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw OrganisationNotFoundException.FromOrganisationId(context.Request.OrganisationId);

        return new GetOrganisationByIdResponse {
            Organisation = OrganisationHelpers.OrganisationFromEntity(organisationEntity),
        };
    }
}
