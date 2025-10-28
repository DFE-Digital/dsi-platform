using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
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
            var response = await directoriesClient.PatchAsJsonAsync($"users/{context.Request.UserId}", new {
                given_name = context.Request.GivenName,
                family_name = context.Request.Surname,
            }, CancellationToken.None);

            response.EnsureSuccessStatusCode();
        }
        catch {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                Type = AuditType.ChangeName,
                Message = $"Failed to change name to {context.Request.GivenName} {context.Request.Surname}",
                UserId = context.Request.UserId,
                Success = false,
            }, CancellationToken.None);
            throw;
        }

        await interaction.DispatchAsync(new WriteToAuditRequest {
            Type = AuditType.ChangeName,
            Message = $"Successfully changed name to {context.Request.GivenName} {context.Request.Surname}",
            UserId = context.Request.UserId,
        }, CancellationToken.None);

        return new ChangeNameResponse();
    }
}
