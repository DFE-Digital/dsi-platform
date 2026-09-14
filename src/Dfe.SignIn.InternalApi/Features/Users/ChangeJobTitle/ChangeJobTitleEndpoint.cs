using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeJobTitle;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class ChangeJobTitleEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    IEventPublisher eventPublisher,
    ILogger<ChangeJobTitleEndpoint> logger) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangeJobTitle, async (
            [FromRoute] Guid userId,
            [FromBody] ChangeJobTitleRequest request,
            [FromServices] ChangeJobTitleEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, request, cancellationToken))
            .WithName("Change Job Title")
            .WithTags("Users")
            .WithStandardResponses()
            .WithValidationFilter<ChangeJobTitleRequest>();
    }

    /// <summary>
    /// Changes the job title of a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose job title is to be changed.</param>
    /// <param name="request">The request containing the user ID and new job title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IResult> HandleAsync(
        Guid userId,
        ChangeJobTitleRequest request,
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

        try {
            await eventPublisher.PublishAsync(new UserUpdatedEvent {
                UserId = user.Sub,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Status = user.Status
            }, cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error publishing UserUpdatedEvent for user {UserId}", userId);
        }

        return Results.Ok();
    }
}
