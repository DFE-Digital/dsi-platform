using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to initiate the change of a user's email address.
/// </summary>
public sealed class ConfirmChangeEmailAddressEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map( IEndpointRouteBuilder app )
    {
        //app.MapPost(UsersApiRoutes.InitiateChangeEmail, Handler)
        //    .WithName("Initiate Change Email Address")
        //    .WithTags("Users")
        //    .Produces(StatusCodes.Status200OK)
        //    .Produces(StatusCodes.Status400BadRequest)
        //    .Produces(StatusCodes.Status404NotFound)
        //    .Produces(StatusCodes.Status401Unauthorized)
        //    .WithValidationFilter<InitiateChangeEmailAddressRequest>()
        //    .WithOpenApi();
    }

    /// <inheritdoc/>
    public static async Task<IResult> Handler(
        IAuditWriter auditWriter,
        IUserLookupService userLookupService,
        IUserCodeService userCodeService,
        IInteractionLimiter actionRateLimiter,
        IOptionsMonitor<DistributedCacheInteractionLimiterOptions> limiterOptions,
        ILogger<InitiateChangeEmailAddressEndpoint> logger,
        [FromBody] InitiateChangeEmailAddressRequest request,
        [FromRoute] Guid userId,
        CancellationToken cancellationToken )
    {
        //logger.LogInformation("Initiating change of email address for user {UserId}", userId);

        //var existingUserInfo = await userLookupService.GetUserInfoAsync(userId, cancellationToken);
        //var existingUserWithNewEmailStatus = await userLookupService.GetUserStatusByEmailAddressAsync(request.NewEmailAddress, cancellationToken);

        //if (existingUserInfo is null) {
        //    logger.LogWarning("User {UserId} not found when attempting to change email address", userId);
        //    return Results.NotFound(new { Message = "User not found" });
        //}

        //if (existingUserWithNewEmailStatus.UserExists) {

        //    if (existingUserWithNewEmailStatus.UserId == existingUserInfo.UserId) {
        //        return Results.BadRequest(new { Message = "Input an email address that is different from your current email address." });
        //    }

        //    // the email address is already in use by another user
        //    await auditWriter.Log(new WriteToAuditRequest {
        //        EventCategory = AuditEventCategoryNames.ChangeEmail,
        //        EventName = AuditChangeEmailEventNames.RequestedExistingEmail,
        //        Message = $"Request to change email from {existingUserInfo.EmailAddress} to existing user {request.NewEmailAddress}",
        //        UserId = existingUserInfo.UserId,
        //    });

        //    return Results.BadRequest(new { Message = "The email address is already in use by another" });
        //}

        //try {
        //    await actionRateLimiter.LimitAndThrowAsync(request, existingUserInfo.UserId.ToString());
        //}
        //catch (InteractionRejectedByLimiterException ex) {
        //    logger.LogWarning(ex, "Rate limit exceeded for user {UserId} when attempting to change email address", existingUserInfo.UserId);
        //    return GetLimitExceededResult(limiterOptions.Get<InitiateChangeEmailAddressRequest>());
        //}

        //await auditWriter.Log(new WriteToAuditRequest {
        //    EventCategory = AuditEventCategoryNames.ChangeEmail,
        //    EventName = AuditChangeEmailEventNames.RequestToChangeEmail,
        //    Message = $"Request to change email from {existingUserInfo.EmailAddress} to {request.NewEmailAddress}",
        //    UserId = existingUserInfo.UserId,
        //});

        //await userCodeService.DeleteExistingCodesAsync(existingUserInfo.UserId, cancellationToken);
        //await userCodeService.CreateNewVerificationCodeAsync(existingUserInfo, request.NewEmailAddress, request.ClientId, cancellationToken);

        //logger.LogInformation("Successfully initiated change of email address for user {UserId}", existingUserInfo.UserId);

        return Results.Ok();
    }
}
