using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeJobTitle;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class ChangeJobTitleEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangeJobTitle, Handler)
            .WithName("Change Job Title")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithValidationFilter<ChangeJobTitleRequest>()
            .WithOpenApi();
    }

    /// <summary>
    /// Changes the job title of a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose job title is to be changed.</param>
    /// <param name="directoriesDbContext">The database context to use for accessing user data.</param>
    /// <param name="auditWriter">The audit writer to log audit events.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="request">The request containing the user ID and new job title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        [FromRoute] Guid userId,
        [FromBody] ChangeJobTitleRequest request,
        DbDirectoriesContext directoriesDbContext,
        IAuditWriter auditWriter,
        ILogger<ChangeJobTitleEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", userId);
            return Results.NotFound();
        }

        if (user.JobTitle == request.NewJobTitle) {
            return Results.Ok();
        }

        var normalisedJobTitle = request.NewJobTitle?.NormalizeWhitespace();

        user.JobTitle = normalisedJobTitle;

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeJobTitle,
            Message = $"Successfully changed job title to {normalisedJobTitle}",
            UserId = userId,
        });

        return Results.Ok();
    }
}
