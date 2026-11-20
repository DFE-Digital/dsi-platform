using Dfe.SignIn.NodeApi.Client;

namespace Dfe.SignIn.InternalApi.Infrastructure.Search;

/// <summary>
/// Removes invitation records from the search index.
/// </summary>
public interface IInvitationSearchRemover
{
    /// <summary>
    /// Removes the invitation's search index record.
    /// </summary>
    /// <param name="userId">The user ID used for logging.</param>
    /// <param name="invitationId">The invitation ID identifying the search record.</param>
    /// <returns></returns>
    Task Handle(Guid userId, Guid invitationId);
}

/// <summary>
/// Removes invitation records through the Search API.
/// </summary>
public class InvitationSearchRemover([FromKeyedServices(NodeApiName.Search)] HttpClient searchClient,
    ILogger<InvitationSearchRemover> logger) : IInvitationSearchRemover
{
    /// <summary>
    /// Removes the invitation's search index record.
    /// </summary>
    /// <param name="userId">The user ID used for logging.</param>
    /// <param name="invitationId">The invitation ID identifying the search record.</param>
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
