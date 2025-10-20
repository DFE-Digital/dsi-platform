using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.SignIn.WebFramework.Mvc.Policies;

/// <summary>
/// Represents an authorization requirement that checks whether the current user
/// is classified as an internal user.
/// </summary>
public sealed class InternalUserRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Indicates whether the requirement is for the user to be internal.
    /// </summary>
    public required bool IsInternalUser { get; init; }
}

/// <summary>
/// Handles the evaluation of <see cref="InternalUserRequirement"/> by checking
/// the user's profile information from the current HTTP context.
/// </summary>
public sealed class InternalUserRequirementHandler(
    IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<InternalUserRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, InternalUserRequirement requirement)
    {
        var userProfileFeature = httpContextAccessor.HttpContext?.Features.Get<IUserProfileFeature>();

        if (userProfileFeature is null) {
            context.Fail();
        }
        else if (userProfileFeature.IsInternalUser == requirement.IsInternalUser) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
