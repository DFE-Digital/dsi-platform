using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;
using Microsoft.AspNetCore.Authentication;

namespace Dfe.SignIn.Web.Profile;

/// <summary>
/// Component runs after the user has been authenicated and is responsible for adding or adjusting application
/// based claims.
/// </summary>
/// <param name="usersApiClient"></param>
public class ApplicationClaimsTransformation(IUsersApiClient usersApiClient, ILogger<ApplicationClaimsTransformation> logger) : IClaimsTransformation
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

        //Safe to set to empty since Guid.Parse will fail if not valid guid
        //and if its missing, the GetUserId() will also throw an exception
        //if it fails to parse.
        Guid userId = Guid.Empty;
        if (principal.Claims.Any(c => c.Type == DsiClaimTypes.UserId)) {
            userId = Guid.Parse(principal.Claims.First(c => c.Type == DsiClaimTypes.UserId).Value);
        }
        else {
            userId = principal.GetUserId();
        }

        var response = await usersApiClient.IsApprover(userId, CancellationToken.None);

        if (response.IsApprover) {
            if (!identity.HasClaim(c => c.Type == OrganisationRoles.Approver.Name)) {
                identity.AddClaim(new Claim(OrganisationRoles.Approver.Name, string.Empty));
            }
        }

        return principal;
    }
}
