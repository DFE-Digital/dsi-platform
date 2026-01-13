using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.InternalApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the name of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class ChangeNameNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<ChangeNameRequest, ChangeNameResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeNameResponse> InvokeAsync(
        InteractionContext<ChangeNameRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        try {
            var response = await directoriesClient.SendAsJsonAsync(HttpMethod.Patch, $"users/{context.Request.UserId}", new {
                given_name = context.Request.FirstName,
                family_name = context.Request.LastName,
            }, "ChangeNameRequest", CancellationToken.None);

            response.EnsureSuccessStatusCode();
        }
        catch {
            await interaction.DispatchAsync(
                new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeName,
                    Message = $"Failed to change name to {context.Request.FirstName} {context.Request.LastName}",
                    UserId = context.Request.UserId,
                    WasFailure = true,
                }
            );
            throw;
        }

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeName,
                Message = $"Successfully changed name to {context.Request.FirstName} {context.Request.LastName}",
                UserId = context.Request.UserId,
            }
        );

        return new ChangeNameResponse();
    }
}
