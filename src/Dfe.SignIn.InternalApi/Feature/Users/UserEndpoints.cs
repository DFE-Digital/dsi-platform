using Dfe.SignIn.InternalApi.Feature.Users.ChangeJobTitle;
using Dfe.SignIn.InternalApi.Feature.Users.GetUserProfile;

namespace Dfe.SignIn.InternalApi.Feature.Users;

public static class UserEndpoints
{
    /// <inheritdoc/>
    public static void UseMyUserEndpoints(this IEndpointRouteBuilder app)
    {
        ChangeJobTitleEndpoint.Map(app);
        GetUserProfileEndpoint.Map(app);
    }
}
