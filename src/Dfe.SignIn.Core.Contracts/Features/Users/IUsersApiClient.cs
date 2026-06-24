using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Refit;

namespace Dfe.SignIn.Core.Contracts.Features.Users;

public interface IUsersApiClient
{
    [Post(ApiRoutes.ChangeJobTitle)]
    Task ChangeJobTitle([Body] ChangeJobTitleRequest request);
}
