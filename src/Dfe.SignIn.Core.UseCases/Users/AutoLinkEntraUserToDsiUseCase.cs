using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for automatically linking an Entra user to a DfE Sign-in user.
/// </summary>
/// <param name="interaction">Service to dispatch interaction requests.</param>
public sealed class AutoLinkEntraUserToDsiUseCase(
    IInteractionDispatcher interaction
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
            return null;
        }

        // User exists in the system; are they an active user though?
        if (userStatusResponse.AccountStatus != AccountStatus.Active) {
            throw new CannotLinkInactiveUserException();
        }

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
            return null;
        }

        // User exists in the system; are they an active user though?
        if (userStatusResponse.AccountStatus != AccountStatus.Active) {
            throw new CannotLinkInactiveUserException();
        }

        await interaction.DispatchAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = userStatusResponse.UserId.Value,
                EntraUserId = request.EntraUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
            }
        ).To<LinkEntraUserToDsiResponse>();

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
            return completeAnyPendingInvitationResponse.UserId.Value;
        }

        // Create new user in system and link to the associated Entra user.
        var createUserResponse = await interaction.DispatchAsync(
            new CreateUserRequest {
                EntraUserId = request.EntraUserId,
                EmailAddress = request.EmailAddress,
                FirstName = request.FirstName,
                LastName = request.LastName,
            }
        ).To<CreateUserResponse>();

        return createUserResponse.UserId;
    }
}
