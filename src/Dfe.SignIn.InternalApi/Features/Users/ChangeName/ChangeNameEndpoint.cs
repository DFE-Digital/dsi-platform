using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.OperationResults;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeName;
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
    IEntraChangeNameService entraChangeNameService,
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

        var normalizedFirstName = request.FirstName.NormalizeWhitespace();
        var normalizedLastName = request.LastName.NormalizeWhitespace();

        if (user.FirstName == normalizedFirstName && user.LastName == normalizedLastName) {
            logger.LogInformation("No changes detected for user {UserId}. FirstName and LastName are the same.", userId);
            return Results.Ok();
        }

        var originalFirstName = user.FirstName;
        var originalLastName = user.LastName;

        user.FirstName = normalizedFirstName;
        user.LastName = normalizedLastName;

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        if (user.IsEntraUser()) {
            var syncResult = await this.TrySyncEntraNameAsync(user, originalFirstName, originalLastName, cancellationToken);
            if (syncResult.IsFailure) {

                await auditWriter.Log(new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeName,
                    Message = $"Failed to change name to {normalizedFirstName} {normalizedLastName} (id: {user.Sub})",
                    UserId = user.Sub,
                    WasFailure = true
                });

                return Results.Problem(
                    detail: syncResult.Error.Description,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeName,
            Message = $"Successfully changed users name to {user.FirstName} {user.LastName}",
            UserId = user.Sub,
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

    private async Task<OperationResult> TrySyncEntraNameAsync(
        UserEntity user,
        string originalFirstName,
        string originalLastName,
        CancellationToken cancellationToken)
    {
        var entraUpdateResult = await entraChangeNameService.ChangeNameAsync(user.EntraOid!.Value, user.FirstName, user.LastName, cancellationToken);

        if (entraUpdateResult.IsSuccess) {
            return OperationResult.Success();
        }

        logger.LogError(
            "Failed to change name in Entra for user {UserId} (EntraOid: {EntraOid}): {Error}. Rolling back database change.",
            user.Sub,
            user.EntraOid.Value,
            entraUpdateResult.Error.Description);

        user.FirstName = originalFirstName;
        user.LastName = originalLastName;

        try {
            await directoriesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception rollbackEx) {
            logger.LogCritical(
                rollbackEx,
                "CRITICAL: Failed to roll back database write for user {UserId} after Entra sync failure!",
                user.Sub);
        }

        return entraUpdateResult;
    }
}
