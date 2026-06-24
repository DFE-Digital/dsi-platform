using Dfe.SignIn.PrivateApi.Responses;
namespace Dfe.SignIn.FauApi;

/// <summary>
/// User queries class.
/// </summary>
public class UsersApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// User queries enpoints
    /// </summary>
    /// <param name="httpClient"></param>
    public UsersApiClient(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    /// <summary>
    /// Get user organisation services and roles.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="clientId">Then service or application name.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetUserOrganisationServicesResponse> GetUserOrganisationServicesAsync(
        Guid userId,
        string? clientId,
        CancellationToken cancellationToken = default)
    {
        using var response = await this._httpClient.GetAsync($"users/{userId}/organisationservices/{clientId}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var model = await response.Content
            .ReadFromJsonAsync<GetUserOrganisationServicesResponse>(cancellationToken: cancellationToken);

        return model ?? throw new InvalidOperationException(
            $"Received null response for user {userId} from FAU API.");
    }
}
