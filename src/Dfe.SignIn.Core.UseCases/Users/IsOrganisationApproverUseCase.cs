using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for checking if a user is an approver for any of their associated organisations.
/// </summary>
public sealed class IsOrganisationApproverUseCase(DbOrganisationsContext unitOfWork)
    : Interactor<IsOrganisationApproverRequest, IsOrganisationApproverResponse>
{
    /// <inheritdoc/>
    public override async Task<IsOrganisationApproverResponse> InvokeAsync(
        InteractionContext<IsOrganisationApproverRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var isApprover = await unitOfWork.UserOrganisations
            .Where(x => x.UserId == context.Request.UserId)
            .AnyAsync(x => x.RoleId == OrganisationRoles.Approver.Id, cancellationToken);

        return new IsOrganisationApproverResponse(isApprover);
    }
}
