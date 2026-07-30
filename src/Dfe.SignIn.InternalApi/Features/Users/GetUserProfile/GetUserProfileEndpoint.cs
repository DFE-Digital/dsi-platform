using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.GetUserProfile;

/// <summary>
/// An endpoint to get the profile of a user.
/// </summary>
public sealed class GetUserProfileEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.GetUserProfile, Handler)
            .WithName("Get User Profile")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi();
    }

    /// <summary>
    /// Gets the profile of a user.
    /// </summary>
    /// <param name="directoriesDbContext">The database context to use for accessing user data.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="userId">The ID of the user to get the profile for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        DbDirectoriesContext directoriesDbContext,
        ILogger<GetUserProfileEndpoint> logger,
        Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting profile for user {UserId}", userId);

        var user = await directoriesDbContext.Users
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

        return Results.Ok(response);
    }
}
