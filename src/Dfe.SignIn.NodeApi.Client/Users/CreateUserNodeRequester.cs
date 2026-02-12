using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Search;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to complete any pending invitation for a given user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class CreateUserNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<CreateUserRequest, CreateUserResponse>
{
    /// <inheritdoc/>
    public override async Task<CreateUserResponse> InvokeAsync(
        InteractionContext<CreateUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await directoriesClient.PostAsJsonAsync("users", new CreateUserRequestDto {
            Email = context.Request.EmailAddress,
            FirstName = context.Request.FirstName,
            LastName = context.Request.LastName,
            Password = null,
            EntraOid = context.Request.EntraUserId,
        }, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var user = (await response.Content.ReadFromJsonAsync<CreateUserResponseDto>(CancellationToken.None))!;

        await interaction.DispatchAsync(
            new UpdateUserInSearchIndexRequest {
                UserId = user!.Id
            }
        );

        return new CreateUserResponse {
            UserId = user.Id,
        };
    }
}
