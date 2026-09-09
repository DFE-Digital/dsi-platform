using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to retrieve a pending change email request for a user.
/// </summary>
public sealed class GetPendingChangeEmailEndpoint(
    IUserCodeService userCodeService,
    TimeProvider timeProvider,
    ILogger<GetPendingChangeEmailEndpoint> logger
) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(UsersApiRoutes.GetPendingChangeEmail, async (
            [FromRoute] Guid userId,
            [FromServices] GetPendingChangeEmailEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, cancellationToken))
            .WithName("Get Pending Change Email")
            .WithTags("Users")
            .WithStandardResponses<GetPendingChangeEmailResponse>();
    }

    /// <summary>
    /// Handles the retrieval of a pending change email request.
    /// </summary>
    public async Task<IResult> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting pending change email for user {UserId}", userId);

        var pendingCode = await userCodeService.GetPendingChangeEmailCodeAsync(userId, cancellationToken);

        if (pendingCode is null || string.IsNullOrWhiteSpace(pendingCode.Email)) {
            logger.LogInformation("No pending change email request found for user {UserId}", userId);
            return Results.NotFound(new { Message = "No pending change email request found" });
        }

        var expiryTime = pendingCode.CreatedAt.AddHours(ChangeEmailConstants.VerificationCodeExpiryHours);
        var hasExpired = timeProvider.GetUtcNow().UtcDateTime > expiryTime;

        return Results.Ok(new GetPendingChangeEmailResponse {
            NewEmailAddress = pendingCode.Email,
            CreatedAtUtc = pendingCode.CreatedAt,
            ExpiryTimeUtc = expiryTime,
            HasExpired = hasExpired,
        });
    }
}
