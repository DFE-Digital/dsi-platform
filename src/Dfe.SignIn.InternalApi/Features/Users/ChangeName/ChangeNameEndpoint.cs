using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeName;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class ChangeNameEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("ApiRoutes.ChangeName", Handler)
            .WithName("Change Name")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<ChangeNameRequest>()
            .WithOpenApi();
    }

    /// <summary>
    /// Changes the name of a user.
    /// </summary>
    /// <param name="dbDirectoriesContext">The database context to use for accessing user data.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="query">The request containing the user ID and new name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        DbDirectoriesContext dbDirectoriesContext,
        ILogger<ChangeNameEndpoint> logger,
        [FromBody] ChangeNameRequest query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Changing name for user {UserId}",
            query.UserId);

        var user = await dbDirectoriesContext
            .Users
            .Where(x => x.Sub == query.UserId)
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new Exception($"User with ID {query.UserId} not found.");
        //throw UserNotFoundException.FromUserId(query.UserId);
        //if (user.JobTitle == query.NewJobTitle) {
        //    logger.LogInformation(
        //        "Job title unchanged for user {UserId} — already set to requested value",
        //        query.UserId);
        //    return Results.Ok(); //TODO: Consider returning a different status code or message indicating that the job title is already set to the desired value.
        //}
        //var normalisedJobTitle = query.NewJobTitle.NormalizeWhitespace();
        //user.JobTitle = normalisedJobTitle;
        //await dbDirectoriesContext.SaveChangesAsync(cancellationToken);
        //logger.LogInformation(
        //    "Successfully changed job title for user {UserId}",
        //    query.UserId);

        return Results.Ok();
    }
}
