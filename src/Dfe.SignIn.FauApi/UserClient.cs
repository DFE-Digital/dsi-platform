using Dfe.SignIn.PrivateApi.Responses;
namespace Dfe.SignIn.FauApi;

/// <summary>
/// User queries class.
/// </summary>
public class UserClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// User queries enpoints
    /// </summary>
    /// <param name="httpClient"></param>
    public UserClient(HttpClient httpClient)
    {
        this._httpClient = httpClient;
        this._httpClient.BaseAddress = new Uri("http://localhost:5003/");
    }

    /// <summary>
    /// Get user organisation services and roles.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="clientId">Then service or application name.</param>
    /// <returns></returns>
    public async Task<GetUserOrganisationServicesResponse> GetUserOrganisationServicesAsync(Guid userId, string? clientId)
    {
        GetUserOrganisationServicesResponse model = await this._httpClient.GetFromJsonAsync<GetUserOrganisationServicesResponse>($"users/{userId}/organisationservices/{clientId}")
                ?? throw new InvalidOperationException($"Received null response for user {userId} from FAU API.");

        return model;
    }
}
