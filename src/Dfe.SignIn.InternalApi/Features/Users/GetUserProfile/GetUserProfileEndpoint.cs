using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.GetUserProfile;

/// <summary>
/// An endpoint to get the profile of a user.
/// </summary>
public sealed class GetUserProfileEndpoint(
    DbDirectoriesContext directoriesDbContext,
        ILogger<GetUserProfileEndpoint> logger) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(UsersApiRoutes.GetUserProfile, async (
            [FromRoute] Guid userId,
            [FromServices] GetUserProfileEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, cancellationToken))
            .WithName("Get User Profile")
            .WithTags("Users")
            .WithStandardResponses();
    }

    /// <summary>
    /// Gets the profile of a user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the profile for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IResult> HandleAsync(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting profile for user {UserId}", userId);

        var user = await directoriesDbContext.Users
            .AsNoTracking()
            .Where(x => x.Sub == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", userId);
            return Results.NotFound();
        }

        var response = new GetUserProfileResponse(
            user.Sub,
            user.FirstName,
            user.LastName,
            user.Email,
            user.IsEntra,
            user.IsInternalUser,
            !string.IsNullOrWhiteSpace(user.JobTitle) ? user.JobTitle : null,
            user.Status
        );

        logger.LogInformation("Returning profile for user {UserId}", userId);

        return Results.Ok(response);
    }
}
