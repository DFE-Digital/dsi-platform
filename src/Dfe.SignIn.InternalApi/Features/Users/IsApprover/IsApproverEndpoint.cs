using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.IsApprover;

/// <summary>
/// An endpoint exposing user approval permissions
/// </summary>
public class IsApproverEndpoint(
        DbOrganisationsContext organisationsDbContext,
        ILogger<IsApproverEndpoint> logger
    ) : IEndpoint
{
    /// <summary>
    /// Endpoint mapping method holding configuration.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(UsersApiRoutes.IsApprover, async (
            [FromRoute] Guid userId,
            [FromServices] IsApproverEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, cancellationToken))
            .WithName("Is Approver")
            .WithTags("Users")
            .WithStandardResponses()
            .RequireAuthorization();
    }

    /// <summary>
    /// Changes the name of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to check for approver status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IsOrganisationApproverResponse> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking if user {userId} is an approver ", userId);

        var isApprover = await organisationsDbContext.UserOrganisations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AnyAsync(x => x.RoleId == OrganisationRole.Approver.Value, cancellationToken);

        logger.LogInformation("User {userId} is approver status: {isApprover}", userId, isApprover);

        return new IsOrganisationApproverResponse(isApprover);
    }
}
