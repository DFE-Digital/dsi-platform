using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Features.Users.ChangeName;
using Dfe.SignIn.WebFramework.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.IsApprover;

/// <summary>
/// An endpoint exposing user approval permissions
/// </summary>
public class IsApproverEndpoint : IEndpoint
{
    /// <summary>
    /// Endpoint mapping method holding configuration.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(UsersApiRoutes.IsApprover, Handler)
            .WithName("Is Approver")
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
    /// <param name="principal">The claims principle belonging to the logged in user</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IsOrganisationApproverResponse> Handler(
        DbOrganisationsContext organisationsDbContext,
        ClaimsPrincipal principal,
        ILogger<ChangeNameEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();

        logger.LogInformation("Checking if user {0} is an approver ", userId);

        var isApprover = await organisationsDbContext.UserOrganisations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AnyAsync(x => x.RoleId == OrganisationRoles.Approver.Id, cancellationToken);

        logger.LogInformation("User {0} Is approver status: {1}", userId, isApprover);

        return new IsOrganisationApproverResponse(isApprover);
    }
}
