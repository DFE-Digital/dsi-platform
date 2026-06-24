using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Refit;

namespace Dfe.SignIn.Core.Contracts.Features.Users;

public interface IUsersApiClient
{
    [Post(ApiRoutes.ChangeJobTitle)]
    Task ChangeJobTitle([Body] ChangeJobTitleRequest request);

    [Post(ApiRoutes.GetUserProfile)]
    Task<GetUserProfileResponseA> GetUserProfile([Body] GetUserProfileRequestA request);
}
