using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// User case to get the number of pending approvals for organisations and
/// services for a approval user
/// </summary>
/// <param name="unitOfWork"></param>
public sealed class PendingApprovalCountUseCase(IUnitOfWorkOrganisations unitOfWork) : Interactor<GetPendingApprovalCountRequest, PendingApprovalCountResponse>
{
    /// <summary>
    /// Calculates the number of outstanding approvals that the Approval user for the given
    /// organisation has.
    ///
    /// This includes pending organisation and service access requests.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<PendingApprovalCountResponse> InvokeAsync(InteractionContext<GetPendingApprovalCountRequest> context, CancellationToken cancellationToken = default)
    {
        var orgIds = await unitOfWork.Repository<UserOrganisationEntity>().Include(x => x.Organisation)
            .Where(x => x.UserId == context.Request.UserId && x.RoleId == OrganisationRoles.Approver.Id)
            .Select(x => x.OrganisationId)
            .ToListAsync(cancellationToken);

        var pendingServiceNotificationCount = await unitOfWork.Repository<UserServiceRequestEntity>()
            .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingOrganisationNotificationCount = await unitOfWork.Repository<UserOrganisationRequestEntity>()
      .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingCount = pendingServiceNotificationCount + pendingOrganisationNotificationCount;

        return new PendingApprovalCountResponse { Count = pendingCount };
    }
}
