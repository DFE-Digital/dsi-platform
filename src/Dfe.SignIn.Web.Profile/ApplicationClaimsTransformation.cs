using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Microsoft.AspNetCore.Authentication;

namespace Dfe.SignIn.Web.Profile;

/// <summary>
/// Component runs after the user has been authenicated and is responsible for adding or adjusting application
/// based claims.
/// </summary>
/// <param name="usersApiClient"></param>
public class ApplicationClaimsTransformation(IUsersApiClient usersApiClient) : IClaimsTransformation
{
    /// <summary>
    /// Transforms the current claims principal and adds claims if required
    /// </summary>
    /// <param name="principal"></param>
    /// <returns>New claims principle with added claims</returns>
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is null) {
            return principal;
        }

        if (!principal.Identity.IsAuthenticated) {
            return principal;
        }

        var identity = (ClaimsIdentity)principal.Identity;

        var userId = principal.GetUserId();
        var response = await usersApiClient.IsApprover(userId, CancellationToken.None);

        if (response.IsApprover) {
            if (!identity.HasClaim(c => c.Type == OrganisationRoles.Approver.Name)) {
                identity.AddClaim(new Claim(OrganisationRoles.Approver.Name, string.Empty));
            }
        }

        return principal;
    }
}
