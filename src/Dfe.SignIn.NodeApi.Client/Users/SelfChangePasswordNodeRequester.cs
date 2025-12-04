using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Interfaces.Graph;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the password of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class SelfChangePasswordNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction,
    IGraphApiChangeUserPassword graphApiChangeUserPassword
) : Interactor<SelfChangePasswordRequest, SelfChangePasswordResponse>
{
    /// <inheritdoc/>
    public override async Task<SelfChangePasswordResponse> InvokeAsync(
        InteractionContext<SelfChangePasswordRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        try {
            if (context.Request.GraphAccessToken is not null) {
                await graphApiChangeUserPassword.ChangePassword(context);
            }
            else {
                await this.CheckCurrentPasswordAsync(context, cancellationToken);

                var response = await directoriesClient.PostAsJsonAsync($"/users/{context.Request.UserId}/changepassword", new {
                    password = context.Request.NewPassword,
                }, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }
        catch {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                Message = "Failed changed password",
                UserId = context.Request.UserId,
                WasFailure = true,
            }, CancellationToken.None);
            throw;
        }

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            Message = "Successfully changed password",
            UserId = context.Request.UserId,
        }, CancellationToken.None);

        return new SelfChangePasswordResponse();
    }

    private async Task CheckCurrentPasswordAsync(
        InteractionContext<SelfChangePasswordRequest> context,
        CancellationToken cancellationToken)
    {
        var profile = await interaction.DispatchAsync(new GetUserProfileRequest {
            UserId = context.Request.UserId,
        }, cancellationToken).To<GetUserProfileResponse>();

        try {
            var response = await directoriesClient.PostAsJsonAsync($"users/authenticate", new {
                username = profile.EmailAddress,
                password = context.Request.CurrentPassword,
            }, CancellationToken.None);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) {
            if (ex.HttpRequestError == HttpRequestError.ConnectionError) {
                await this.ReportIncorrectCurrentPasswordAsync(context);
            }
            throw;
        }
    }

    private async Task ReportIncorrectCurrentPasswordAsync(InteractionContext<SelfChangePasswordRequest> context)
    {
        context.AddValidationError(
            "The password you entered was not recognised",
            nameof(context.Request.CurrentPassword)
        );

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            EventName = AuditChangePasswordEventNames.IncorrectPassword,
            Message = $"Failed changed password - Incorrect current password",
            UserId = context.Request.UserId,
            WasFailure = true,
        }, CancellationToken.None);

        context.ThrowIfHasValidationErrors();
    }
}
