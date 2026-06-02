using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.AspNetCore.Authentication;

namespace Dfe.SignIn.Web.Profile;

/// <summary>
/// Component runs after the user has been authenicated and is responsible for adding or adjusting application
/// based claims.
/// </summary>
/// <param name="interaction"></param>
public class ApplicationClaimsTransformation(IInteractionDispatcher interaction) : IClaimsTransformation
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

        var response = await interaction.DispatchAsync(new IsOrganisationApproverRequest(principal.GetUserId()))
            .To<IsOrganisationApproverResponse>();

        if (response.IsApprover) {
            if (!identity.HasClaim(c => c.Type == OrganisationRoles.Approver.Name)) {
                identity.AddClaim(new Claim(OrganisationRoles.Approver.Name, string.Empty));
            }
        }

        return principal;
    }
}
