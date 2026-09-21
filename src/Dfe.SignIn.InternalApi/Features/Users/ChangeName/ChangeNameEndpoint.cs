using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeName;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class ChangeNameEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    IEventPublisher eventPublisher,
    ILogger<ChangeNameEndpoint> logger) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangeName, async (
            [FromRoute] Guid userId,
            [FromBody] ChangeNameRequest request,
            [FromServices] ChangeNameEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, request, cancellationToken))
            .WithName("Change Name")
            .WithTags("Users")
            .WithStandardResponses()
            .WithValidationFilter<ChangeNameRequest>();
    }

    /// <summary>
    /// Changes the name of a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request">The request containing the user ID and new name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IResult> HandleAsync(
        Guid userId,
        ChangeNameRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing name for user {UserId}", userId);

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", userId);
            return Results.NotFound();
        }

        if (user.FirstName == request.FirstName && user.LastName == request.LastName) {
            logger.LogInformation("No changes detected for user {UserId}. FirstName and LastName are the same.", userId);
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

        logger.LogInformation("Successfully changed name for user {UserId}", userId);

        return Results.Ok();
    }
}
