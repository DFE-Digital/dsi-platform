using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to cancel the change of a user's email address.
/// </summary>
public sealed class CancelChangeEmailAddressEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    ILogger<CancelChangeEmailAddressEndpoint> logger,
    IUserLookupService userLookupService) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(UsersApiRoutes.CancelChangeEmail, async (
            [FromRoute] Guid userId,
            [FromServices] CancelChangeEmailAddressEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, cancellationToken))
            .WithName("Cancel Change Email Address")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi();
    }

    /// <summary>
    /// Handles the cancellation of a user's email address change request.
    /// </summary>
    public async Task<IResult> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling email change for user {UserId}", userId);

        var userInfo = await userLookupService.GetUserInfoAsync(userId, cancellationToken);

        if (userInfo is null) {
            logger.LogWarning("User {UserId} not found", userId);
            return Results.NotFound(new { Message = "User not found" });
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.CancelChangeEmail,
            Message = $"Cancel change email request from {userInfo.EmailAddress} (id: {userId})",
        });

        try {
            await directoriesDbContext.UserCodes
                .Where(uc => uc.Uid == userId)
                .Where(uc => uc.CodeType == UserCodeType.ChangeEmail.ToDbValue())
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error cancelling email change for user {UserId}", userId);
            throw;
        }

        logger.LogInformation("Successfully cancelled email change for user {UserId}", userId);
        return Results.Ok();
    }
}
