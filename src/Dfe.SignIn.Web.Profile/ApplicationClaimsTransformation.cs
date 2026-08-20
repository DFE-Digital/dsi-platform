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
public class ApplicationClaimsTransformation(IUsersApiClient usersApiClient) : IClaimsTransformation
{
    /// <summary>
    /// Transforms the current claims principal and adds claims if required
    /// </summary>
    /// <param name="principal"></param>
    /// <returns>New claims principle with added claims</returns>
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true }) {
            return principal;
        }

        if (principal.HasClaim(c => c.Type == OrganisationRole.Approver.Name)) {
            return principal;
        }

        var identity = (ClaimsIdentity)principal.Identity;

        //Safe to set to empty since Guid.Parse will fail if not valid guid and if its missing, the GetUserId() will also throw an exception if it fails to parse.
        Guid userId = Guid.Empty;

        // The application supports two authentication schemes which use different claim schemas.
        // 1. App-specific users: The user ID is stored in our custom 'DsiClaimTypes.UserId' claim.
        // 2. Entra ID users: The user ID is stored in the standard OIDC 'ClaimTypes.NameIdentifier' claim.
        // We check for the custom app claim first. If it is missing, we fall back to GetUserId(), 
        // which handles extracting the ID from the Entra ID NameIdentifier claim.
        if (principal.Claims.Any(c => c.Type == DsiClaimTypes.UserId)) {
            userId = Guid.Parse(principal.Claims.First(c => c.Type == DsiClaimTypes.UserId).Value);
        }
        else {
            userId = principal.GetUserId();
        }

        var clone = principal.Clone();
        var cloneIdentity = (ClaimsIdentity)clone.Identity!;

        var response = await usersApiClient.IsApprover(userId, CancellationToken.None);

        if (response.IsApprover) {
            cloneIdentity.AddClaim(new Claim(OrganisationRole.Approver.Name, string.Empty));
        }

        return clone;
    }
}
