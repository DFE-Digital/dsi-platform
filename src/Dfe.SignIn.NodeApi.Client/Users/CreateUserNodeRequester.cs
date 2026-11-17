using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Search;
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
public sealed class CreateUserNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction,
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
            FirstName = context.Request.FirstName,
            LastName = context.Request.LastName,
            Password = null,
            EntraOid = context.Request.EntraUserId,
        }, CancellationToken.None);

        if (response.StatusCode == HttpStatusCode.Conflict) {
            // The Directories API found an existing user with this email or Entra identity
            // that is not the same identity retrying (which it returns as a success).
            logger.LogWarning(
                "Directories API refused to create user for Entra account '{EntraUserId}' ({EmailAddress}) due to a conflict.",
                context.Request.EntraUserId, context.Request.EmailAddress
            );
            throw CannotCreateNewUserException.FromEmailAddress(context.Request.EmailAddress);
        }

        if (!response.IsSuccessStatusCode) {
            logger.LogError(
                "Directories API failed to create user for Entra account '{EntraUserId}' ({EmailAddress}). Status: {StatusCode}.",
                context.Request.EntraUserId, context.Request.EmailAddress, (int)response.StatusCode
            );
        }

        response.EnsureSuccessStatusCode();

        var user = (await response.Content.ReadFromJsonAsync<CreateUserResponseDto>(CancellationToken.None))!;

        logger.LogInformation(
            "Directories API returned user '{UserId}' for Entra account '{EntraUserId}'.",
            user.Id, context.Request.EntraUserId
        );

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
