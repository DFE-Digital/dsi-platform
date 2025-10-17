using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to complete any pending invitation for a given user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
[NodeApi(NodeApiName.Organisations)]
[NodeApi(NodeApiName.Access)]
[NodeApi(NodeApiName.Search)]
public sealed class CompleteAnyPendingInvitationNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient organisationsClient,
    [FromKeyedServices(NodeApiName.Access)] HttpClient accessClient,
    [FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<CompleteAnyPendingInvitationNodeRequester> logger
) : Interactor<CompleteAnyPendingInvitationRequest, CompleteAnyPendingInvitationResponse>
{
    /// <inheritdoc/>
    public override async Task<CompleteAnyPendingInvitationResponse> InvokeAsync(
        InteractionContext<CompleteAnyPendingInvitationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var invitation = await this.GetInvitationAsync(context.Request.EmailAddress);
        if (invitation is null || invitation.IsCompleted) {
            return new CompleteAnyPendingInvitationResponse {
                WasCompleted = false,
            };
        }

        var user = await this.ConvertInvitationToUserAsync(invitation.Id, context.Request.EntraUserId);
        logger.LogInformation(
            "Fulfilled pending invitation '{InvitationId}' for user '{UserId}'.",
            invitation.Id, user.Id
        );

        await this.AssignOrganisationsFromInvitationAsync(invitation.Id, user.Id);
        await this.AssignServicesFromInvitationAsync(invitation.Id, user.Id);

        logger.LogInformation(
            "Assigned organisations and services from pending invitation '{InvitationId}' for user '{UserId}'.",
            invitation.Id, user.Id
        );

        await this.RemoveInvitationFromSearchIndexAsync(invitation.Id, user.Id);
        await this.UpdateUserInSearchIndexAsync(invitation.Id, user.Id);

        return new CompleteAnyPendingInvitationResponse {
            WasCompleted = true,
            UserId = user.Id,
        };
    }

    private Task<GetInvitationResponseDto?> GetInvitationAsync(string emailAddress)
    {
        return directoriesClient.GetFromJsonOrDefaultAsync<GetInvitationResponseDto>(
            $"invitations/by-email/{emailAddress}"
        );
    }

    private async Task<ConvertInvitationToUserResponseDto> ConvertInvitationToUserAsync(
        Guid invitationId, Guid entraOid)
    {
        var response = await directoriesClient.PostAsJsonAsync(
            $"invitations/{invitationId}/create_user",
            new ConvertInvitationToUserRequestDto {
                EntraOid = entraOid
            },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ConvertInvitationToUserResponseDto>())!;
    }

    private async Task<AssignOrganisationsFromInvitationResponseDto> AssignOrganisationsFromInvitationAsync(
        Guid invitationId, Guid userId)
    {
        var response = await organisationsClient.PostAsJsonAsync(
            $"invitations/{invitationId}/migrate-to-user",
            new AssignOrganisationsFromInvitationRequestDto {
                UserId = userId,
            }
        );
        response.EnsureSuccessStatusCode();
        return new AssignOrganisationsFromInvitationResponseDto();
    }

    private async Task<AssignServicesFromInvitationResponseDto> AssignServicesFromInvitationAsync(
        Guid invitationId, Guid userId)
    {
        var response = await accessClient.PostAsJsonAsync(
            $"invitations/{invitationId}/migrate-to-user",
            new AssignServicesFromInvitationRequestDto {
                UserId = userId,
            }
        );
        response.EnsureSuccessStatusCode();
        return new AssignServicesFromInvitationResponseDto();
    }

    private async Task RemoveInvitationFromSearchIndexAsync(Guid invitationId, Guid userId)
    {
        // Invitation ID in request path is case sensitive.
        string removeSearchIndexId = $"inv-{invitationId.ToString().ToUpper()}";
        try {
            var response = await searchClient.DeleteAsync($"users/{removeSearchIndexId}");
            response.EnsureSuccessStatusCode();
            logger.LogInformation(
                "Removed '{RemoveSearchIndexId}' from search index for user '{UserId}'.",
                removeSearchIndexId, userId
            );
        }
        catch (Exception ex) {
            logger.LogWarning(
                ex,
                "Unable to remove '{RemoveSearchIndexId}' from search index for user '{UserId}'.",
                removeSearchIndexId, userId
            );
        }
    }

    private async Task UpdateUserInSearchIndexAsync(Guid invitationId, Guid userId)
    {
        try {
            var response = await searchClient.PostAsJsonAsync(
                $"users/update-index",
                new UpdateUserInSearchIndexRequestDto {
                    Id = userId,
                }
            )!;
            response.EnsureSuccessStatusCode();
            logger.LogInformation(
                "Updated search index pending invitation '{InvitationId}' for user '{UserId}'.",
                invitationId, userId
            );
        }
        catch (Exception ex) {
            logger.LogWarning(
                ex,
                "Unable to update search index pending invitation '{InvitationId}' for user '{UserId}'.",
                invitationId, userId
            );
        }
    }
}
