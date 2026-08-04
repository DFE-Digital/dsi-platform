using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class InitialiseChangeEmailAddressEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangeName, Handler)
            .WithName("Change Email Address")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithValidationFilter<InitiateChangeEmailAddressRequest>()
            .WithOpenApi();
    }

    /// <inheritdoc/>
    public static async Task<IResult> Handler(
        IAuditWriter auditWriter,
        IUserLookupService userLookupService,
        IUserCodeService userCodeService,
        IInteractionLimiter actionRateLimiter,
        ILogger<InitialiseChangeEmailAddressEndpoint> logger,
        [FromBody] InitiateChangeEmailAddressRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Initiating change of email address for user {UserId}", request.UserId);

        var existingUserEmail = await userLookupService.GetUserEmailAddressAsync(request.UserId, cancellationToken);
        var existingUserWithNewEmailStatus = await userLookupService.GetUserStatusByEmailAddressAsync(request.NewEmailAddress, cancellationToken);

        if (existingUserWithNewEmailStatus.UserExists) {

            if (existingUserWithNewEmailStatus.UserId == request.UserId) {
                return Results.BadRequest(new { Message = "Input an email address that is different from your current email address." });
            }

            // the email address is already in use by another user
            await auditWriter.Log(new InteractionContext<WriteToAuditRequest>(
                new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeEmail,
                    EventName = AuditChangeEmailEventNames.RequestedExistingEmail,
                    Message = $"Request to change email from {existingUserEmail} to existing user {request.NewEmailAddress}",
                    UserId = request.UserId,
                }));

            return Results.BadRequest(new { Message = "The email address is already in use by another" });
        }

        if (string.IsNullOrEmpty(existingUserEmail)) {
            logger.LogWarning("User {UserId} not found when attempting to change email address", request.UserId);
            return Results.NotFound(new { Message = "User not found" });
        }

        //todo: review this, ideal response should be 429 with a Retry-After header, but the current implementation throws an exception which results in a 500 response
        //  HTTP/1.1 429 Too Many Requests
        //  Content - Type: application / json
        //  Retry - After: 60
        //  {
        //    "error": "Rate limit exceeded",
        //    "message": "You have exceeded your request allowance. Please try again in 60 seconds."
        //  }
        await actionRateLimiter.LimitAndThrowAsync(request);

        //todo: remove the interactioncontext and use the auditwriter directly
        await auditWriter.Log(new InteractionContext<WriteToAuditRequest>(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.RequestToChangeEmail,
                Message = $"Request to change email from {existingUserEmail} to {request.NewEmailAddress}",
                UserId = request.UserId,
            }));

        await userCodeService.DeleteExistingCodesAsync(request.UserId, cancellationToken);
        await userCodeService.CreateNewVerificationCodeAsync(request.UserId, existingUserEmail, request.NewEmailAddress, request.ClientId, cancellationToken);

        logger.LogInformation("Successfully initiated change of email address for user {UserId}", request.UserId);

        return Results.Ok();
    }
}
