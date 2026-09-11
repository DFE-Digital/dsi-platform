using Dfe.SignIn.NodeApi.Client;

namespace Dfe.SignIn.InternalApi.Services.Search;

/// <summary>
/// 
/// </summary>
public class RemoveInviteService([FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<RemoveInviteService> logger)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="invitationId"></param>
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
