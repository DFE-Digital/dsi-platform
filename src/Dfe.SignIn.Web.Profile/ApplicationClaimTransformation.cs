using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.AspNetCore.Authentication;

namespace Dfe.SignIn.Web.Profile;

public class ApplicationClaimTransformation(IInteractionDispatcher interaction) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.Identity.IsAuthenticated) {
            return principal;
        }

        var identity = (ClaimsIdentity)principal.Identity;

        var response = await interaction.DispatchAsync(new IsOrganisationApproverRequest(principal.GetUserId()))
            .To<IsOrganisationApproverResponse>();

        if (!identity.HasClaim(c => c.Type == "is_approver")) {
            identity.AddClaim(new Claim("is_approver", response.IsApprover.ToString()));
        }

        return principal;
    }
}
