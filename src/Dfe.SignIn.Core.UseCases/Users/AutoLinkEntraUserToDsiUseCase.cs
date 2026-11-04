using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for automatically linking an Entra user to a DfE Sign-in user.
/// </summary>
/// <param name="interaction">Service to dispatch interaction requests.</param>
public sealed class AutoLinkEntraUserToDsiUseCase(
    IInteractionDispatcher interaction,
    ILogger<AutoLinkEntraUserToDsiUseCase> logger
) : Interactor<AutoLinkEntraUserToDsiRequest, AutoLinkEntraUserToDsiResponse>
{
    /// <inheritdoc/>
    public override async Task<AutoLinkEntraUserToDsiResponse> InvokeAsync(
        InteractionContext<AutoLinkEntraUserToDsiRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        return new AutoLinkEntraUserToDsiResponse {
            UserId = await this.AutoLinkUserAsync(context.Request),
        };
    }

    private async Task<Guid> AutoLinkUserAsync(AutoLinkEntraUserToDsiRequest request)
    {
        return await this.GetExistingLinkedUserAsync(request)
            ?? await this.LinkToExistingDsiUserAsync(request)
            ?? await this.CreateDsiUserAsync(request);
    }

    private async Task<Guid?> GetExistingLinkedUserAsync(AutoLinkEntraUserToDsiRequest request)
    {
        var userStatusResponse = await interaction.DispatchAsync(
            new GetUserStatusRequest {
                EntraUserId = request.EntraUserId,
            }
        ).To<GetUserStatusResponse>();

        if (!userStatusResponse.UserExists) {
            logger.LogInformation(
                "No DfE Sign-In user is linked to Entra account {EntraUserId}.",
                request.EntraUserId);
            return null;
        }

        // User exists in the system; are they an active user though?
        if (userStatusResponse.AccountStatus != AccountStatus.Active) {
            logger.LogWarning(
                "Entra account {EntraUserId} is linked to DfE Sign-In user {UserId} whose account status is {AccountStatus}.",
                request.EntraUserId, userStatusResponse.UserId, userStatusResponse.AccountStatus);
            throw new CannotLinkInactiveUserException();
        }

        logger.LogInformation(
            "Entra account {EntraUserId} is already linked to DfE Sign-In user {UserId}.",
            request.EntraUserId, userStatusResponse.UserId);
        return userStatusResponse.UserId;
    }

    private async Task<Guid?> LinkToExistingDsiUserAsync(AutoLinkEntraUserToDsiRequest request)
    {
        // Let's try to associate the Entra user object with a DSI account.
        var userStatusResponse = await interaction.DispatchAsync(
            new GetUserStatusRequest {
                EmailAddress = request.EmailAddress,
            }
        ).To<GetUserStatusResponse>();

        if (!userStatusResponse.UserExists) {
            logger.LogInformation(
                "No existing DfE Sign-In user matches the email address of Entra account {EntraUserId}.",
                request.EntraUserId);
            return null;
        }

        // User exists in the system; are they an active user though?
        if (userStatusResponse.AccountStatus != AccountStatus.Active) {
            logger.LogWarning(
                "Existing DfE Sign-In user {UserId} matching Entra account {EntraUserId} has account status {AccountStatus}.",
                userStatusResponse.UserId, request.EntraUserId, userStatusResponse.AccountStatus);
            throw new CannotLinkInactiveUserException();
        }

        logger.LogInformation(
            "Linking Entra account {EntraUserId} to existing DfE Sign-In user {UserId}.",
            request.EntraUserId, userStatusResponse.UserId);

        await interaction.DispatchAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = userStatusResponse.UserId.Value,
                EntraUserId = request.EntraUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
            }
        ).To<LinkEntraUserToDsiResponse>();

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.Auth,
            EventName = AuditAuthEventNames.LinkToExistingUser,
            Message = $"Linked Entra account with existing DfE Sign-In user {request.EmailAddress}",
            UserId = userStatusResponse.UserId,
        });

        return userStatusResponse.UserId.Value;
    }

    private async Task<Guid> CreateDsiUserAsync(AutoLinkEntraUserToDsiRequest request)
    {
        // User does not exist in the system; is there a pending invitation?
        var completeAnyPendingInvitationResponse = await interaction.DispatchAsync(
            new CompleteAnyPendingInvitationRequest {
                EmailAddress = request.EmailAddress,
                EntraUserId = request.EntraUserId,
            }
        ).To<CompleteAnyPendingInvitationResponse>();

        if (completeAnyPendingInvitationResponse.UserId is not null) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkToInvitedUser,
                Message = $"Linked Entra account with pending DfE Sign-In invitation {request.EmailAddress}",
                UserId = completeAnyPendingInvitationResponse.UserId,
            });

            logger.LogInformation(
                "Linked Entra account {EntraUserId} to pending invitation user {UserId}.",
                request.EntraUserId, completeAnyPendingInvitationResponse.UserId);
            return completeAnyPendingInvitationResponse.UserId.Value;
        }

        // Create new user in system and link to the associated Entra user.
        logger.LogInformation(
            "No existing user or pending invitation for Entra account {EntraUserId}; creating a new DfE Sign-In user.",
            request.EntraUserId);

        CreateUserResponse createUserResponse;
        try {
            createUserResponse = await interaction.DispatchAsync(
                new CreateUserRequest {
                    EntraUserId = request.EntraUserId,
                    EmailAddress = request.EmailAddress,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                }
            ).To<CreateUserResponse>();
        }
        catch (CannotCreateNewUserException) {
            // A concurrent request (e.g. a repeated token issuance for the same
            // registration) may have created the DSI user between our lookups and the
            // create. Look again rather than failing and leaving the Entra account
            // orphaned; if there is still no matching user this is a genuine conflict.
            logger.LogWarning(
                "Creating a DfE Sign-In user for Entra account {EntraUserId} was refused as a conflict; checking for a user created concurrently.",
                request.EntraUserId);

            var recoveredUserId = await this.GetExistingLinkedUserAsync(request)
                ?? await this.LinkToExistingDsiUserAsync(request);

            if (recoveredUserId is null) {
                logger.LogError(
                    "Create conflict for Entra account {EntraUserId} but no existing DfE Sign-In user could be found to link to.",
                    request.EntraUserId);
                throw;
            }

            logger.LogInformation(
                "Recovered from create conflict: Entra account {EntraUserId} resolved to DfE Sign-In user {UserId}.",
                request.EntraUserId, recoveredUserId);
            return recoveredUserId.Value;
        }

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.Auth,
            EventName = AuditAuthEventNames.LinkToNewUser,
            Message = $"Linked Entra account with new DfE Sign-In user {request.EmailAddress}",
            UserId = createUserResponse.UserId,
        });

        return createUserResponse.UserId;
    }
}
