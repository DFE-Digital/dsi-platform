using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Refit;

namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Represents a client for interacting with user-related API endpoints.
/// </summary>
public interface IUsersApiClient
{
    /// <summary>
    /// Changes the name of a user based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the user's new name information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Post(UsersApiRoutes.ChangeName)]
    Task ChangeName([Body] ChangeNameRequest request);

    /// <summary>
    /// Retrieves the profile information of a user based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the user's ID and name information.</param>
    /// <returns>A task representing the asynchronous operation, containing the user's profile information.</returns>
    [Get(UsersApiRoutes.GetUserProfile)]
    Task<GetUserProfileResponse> GetUserProfile([Query] GetUserProfileResponse request);
}
