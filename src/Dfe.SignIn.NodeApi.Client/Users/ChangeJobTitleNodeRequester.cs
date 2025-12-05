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
public sealed class ChangeJobTitleNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<ChangeJobTitleRequest, ChangeJobTitleResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeJobTitleResponse> InvokeAsync(
        InteractionContext<ChangeJobTitleRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await directoriesClient.PatchAsJsonAsync($"users/{context.Request.UserId}", new {
            job_title = context.Request.NewJobTitle,
        }, CancellationToken.None);

        response.EnsureSuccessStatusCode();

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeJobTitle,
                Message = $"Successfully changed job title to {context.Request.NewJobTitle}",
                UserId = context.Request.UserId,
            }
        );

        return new ChangeJobTitleResponse();
    }
}
