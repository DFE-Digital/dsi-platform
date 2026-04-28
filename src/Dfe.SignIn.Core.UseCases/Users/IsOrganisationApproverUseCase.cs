using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for checking if a user is an approver for any of their associated organisations.
/// </summary>
public sealed class IsOrganisationApproverUseCase(
    IUnitOfWorkOrganisations unitOfWork
)
    : Interactor<IsOrganisationApproverRequest, IsOrganisationApproverResponse>
{
    /// <inheritdoc/>
    public override async Task<IsOrganisationApproverResponse> InvokeAsync(
        InteractionContext<IsOrganisationApproverRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var approverRoleId = 10000; // This is the role ID for approvers, but this should be moved to a constant somewhere.

        var isApprover = await unitOfWork.Repository<UserOrganisationEntity>()
            .Where(x => x.UserId == context.Request.UserId)
            .AnyAsync(x => x.RoleId == approverRoleId, cancellationToken);

        return new IsOrganisationApproverResponse(isApprover);
    }
}
