using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.PendingApprovalCounter;

/// <summary>
/// 
/// </summary>
public class PendingApprovalCounterEndpoint : IEndpoint
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(UsersApiRoutes.PendingApprovalCounter, Handler)
            .WithName("Pending Approval Counter")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="organisationsDbContext"></param>
    /// <param name="userId"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<PendingApprovalCountResponse> Handler(
        DbOrganisationsContext organisationsDbContext,
        Guid userId,
        ILogger<PendingApprovalCounterEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var orgIds = await organisationsDbContext.UserOrganisations.Include(x => x.Organisation)
                   .Where(x => x.UserId == userId && x.RoleId == OrganisationRoles.Approver.Id)
                   .Select(x => x.OrganisationId)
                   .ToListAsync(cancellationToken);

        var pendingServiceNotificationCount = await organisationsDbContext.UserServiceRequests
            .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingOrganisationNotificationCount = await organisationsDbContext.UserOrganisationRequests
      .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingCount = pendingServiceNotificationCount + pendingOrganisationNotificationCount;

        return new PendingApprovalCountResponse { Count = pendingCount };
    }
}
