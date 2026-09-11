using Dfe.SignIn.NodeApi.Client;

namespace Dfe.SignIn.InternalApi.Services.Search;

/// <summary>
/// An implementation to remove user invitations from DSI
/// </summary>
public class RemoveInviteService([FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<RemoveInviteService> logger)
{
    /// <summary>
    /// Removes the user invitation based on the invitation Id provided
    /// from the DSI database.
    /// </summary>
    /// <param name="userId">ID represeting the user to remove the invitation from</param>
    /// <param name="invitationId">ID represting the users invitation to remove</param>
    /// <returns></returns>
    public async Task Handle(Guid userId, Guid invitationId)
    {
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
}
