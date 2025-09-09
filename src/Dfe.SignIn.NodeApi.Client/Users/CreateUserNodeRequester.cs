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
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class CreateUserNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    [FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<CreateUserNodeRequester> logger
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
            FirstName = context.Request.GivenName,
            LastName = context.Request.Surname,
            Password = null,
            EntraOid = context.Request.EntraUserId,
        });
        response.EnsureSuccessStatusCode();

        var user = (await response.Content.ReadFromJsonAsync<CreateUserResponseDto>())!;

        await this.UpdateUserInSearchIndexAsync(user!.Id);

        return new CreateUserResponse {
            UserId = user.Id,
        };
    }

    private async Task UpdateUserInSearchIndexAsync(Guid userId)
    {
        try {
            var response = await searchClient.PostAsJsonAsync($"users/update-index", new UpdateUserInSearchIndexRequestDto {
                Id = userId,
            })!;
            response.EnsureSuccessStatusCode();
            logger.LogInformation(
                $"Updated search index for user '{userId}'."
            );
        }
        catch {
            logger.LogError(
                $"Unable to update search index for user '{userId}'."
            );
        }
    }
}
