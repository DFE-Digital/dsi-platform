using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.PendingApprovalCounter;

/// <summary>
/// An endpoint exposing user pending approval counts
/// </summary>
public class PendingApprovalCounterEndpoint : IEndpoint
{
    /// <summary>
    /// Endpoint mapping method holding configuration.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to</param>
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
    /// Changes the name of a user.
    /// </summary>
    /// <param name="organisationsDbContext">The database context to use for accessing user data.</param>
    /// <param name="userId">The userId that the request belongs to</param>
    /// <param name="userLookupService">The service to use for looking up user information.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        DbOrganisationsContext organisationsDbContext,
        Guid userId,
        IUserLookupService userLookupService,
        ILogger<PendingApprovalCounterEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var userExists = await userLookupService.UserExists(userId, cancellationToken);
        if (!userExists) {
            return Results.NotFound(new { Message = "User not found" });
        }

        var orgIds = await organisationsDbContext.UserOrganisations
                   .AsNoTracking()
                   .Include(x => x.Organisation)
                   .Where(x => x.UserId == userId)
                   .Where(x => x.RoleId == OrganisationRole.Approver.Value)
                   .Select(x => x.OrganisationId)
                   .ToListAsync(cancellationToken);

        var pendingServiceNotificationCount = await organisationsDbContext.UserServiceRequests
            .AsNoTracking()
            .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingOrganisationNotificationCount = await organisationsDbContext.UserOrganisationRequests
            .AsNoTracking()
            .CountAsync(x => orgIds.Contains(x.OrganisationId) && !x.ActionedAt.HasValue, cancellationToken);

        var pendingCount = pendingServiceNotificationCount + pendingOrganisationNotificationCount;

        return Results.Ok(new PendingApprovalCountResponse { Count = pendingCount });
    }
}
