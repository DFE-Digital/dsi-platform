using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
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
        app.MapPost(UsersApiRoutes.ChangeName, Handler)
            .WithName("Change Name")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithValidationFilter<ChangeNameRequest>()
            .WithOpenApi();
    }

    /// <summary>
    /// Changes the name of a user.
    /// </summary>
    /// <param name="directoriesDbContext">The database context to use for accessing user data.</param>
    /// <param name="auditWriter">The audit writer to log audit events.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="request">The request containing the user ID and new name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        DbDirectoriesContext directoriesDbContext,
        IAuditWriter auditWriter,
        ILogger<ChangeNameEndpoint> logger,
        [FromBody] ChangeNameRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing name for user {UserId}", request.UserId);

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", request.UserId);
            return Results.NotFound();
        }

        if (user.FirstName == request.FirstName && user.LastName == request.LastName) {
            logger.LogInformation("No changes detected for user {UserId}. FirstName and LastName are the same.", request.UserId);
            return Results.Ok();
        }

        if (user.FirstName != request.FirstName) {
            user.FirstName = request.FirstName.NormalizeWhitespace();
        }

        if (user.LastName != request.LastName) {
            user.LastName = request.LastName.NormalizeWhitespace();
        }

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeName,
            Message = $"Successfully changed users name to {user.FirstName} {user.LastName}",
            UserId = request.UserId,
        });

        logger.LogInformation("Successfully changed name for user {UserId}", request.UserId);

        return Results.Ok();
    }
}
