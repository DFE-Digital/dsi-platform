using Dfe.SignIn.Core.Contracts.Features.Applications.GetApplicationByClientId;
using Refit;

namespace Dfe.SignIn.Core.Contracts.Features.Applications;

/// <summary>
/// Defines the contract for an API client that interacts with application-related endpoints, specifically for retrieving application details by client ID.
/// </summary>
public interface IApplicationsApiClient
{
    /// <summary>
    /// Retrieves application details based on the provided client ID.
    /// </summary>
    /// <param name="request">The request containing the client ID.</param>
    /// <returns>A response containing the application details.</returns>
    [Post(ApiRoutes.GetApplicationByClientId)]
    Task<GetApplicationByClientIdResponse> GetApplicationByClientId([Body] GetApplicationByClientIdRequest request);
}
