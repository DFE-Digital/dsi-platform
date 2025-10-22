using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the password of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class SelfChangePasswordNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<SelfChangePasswordRequest, SelfChangePasswordResponse>
{
    /// <inheritdoc/>
    public override async Task<SelfChangePasswordResponse> InvokeAsync(
        InteractionContext<SelfChangePasswordRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        try {
            var response = await directoriesClient.PostAsJsonAsync($"users/authenticate", new {
                username = context.Request.UserId,
                password = context.Request.CurrentPassword,
            }, CancellationToken.None);

            response.EnsureSuccessStatusCode();
        }
        catch {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                EventName = AuditChangePasswordEventNames.IncorrectPassword,
                Message = $"Failed changed password - Incorrect current password",
                UserId = context.Request.UserId,
                CustomProperties = [
                    new("success", false),
                ],
            }, CancellationToken.None);
            throw;
        }

        try {
            var response = await directoriesClient.PatchAsJsonAsync($"users/{context.Request.UserId}", new {
                password = context.Request.NewPassword,
            }, CancellationToken.None);
            response.EnsureSuccessStatusCode();
        }
        catch {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                Message = $"Failed changed password",
                UserId = context.Request.UserId,
                CustomProperties = [
                    new("success", false),
                ],
            }, CancellationToken.None);
            throw;
        }

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            Message = $"Successfully changed password",
            UserId = context.Request.UserId,
            CustomProperties = [
                new("success", true),
            ],
        }, CancellationToken.None);

        return new SelfChangePasswordResponse();
    }
}
