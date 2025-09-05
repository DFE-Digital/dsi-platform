using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.Contracts.Organisations;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// A service that gets or sets the active organisation of a user by setting entries in
/// <see cref="HttpContext.Session"/>.
/// </summary>
/// <remarks>
///   <para>Verifies that the active organisation was stored against the "sid" claim of
///   the primary user identity (the identity with the "dsi_user" claim).</para>
/// </remarks>
public class ActiveOrganisationSessionProvider : IActiveOrganisationProvider
{
    internal const string AssociatedSidKey = "a0f2d391-4878-4302-a6c8-d4e01d7a7378";
    internal const string OrganisationKey = "75da580b-e1df-4969-886a-02a5eb430246";

    private static string GetSidValue(HttpContext context)
    {
        var userIdentity = context.User.GetPrimaryIdentity();
        return userIdentity?.FindFirst(DsiClaimTypes.SessionId)?.Value
            ?? throw new MissingClaimException(null, DsiClaimTypes.SessionId);
    }

    /// <inheritdoc/>
    public virtual Task SetActiveOrganisationAsync(IHttpContext context, OrganisationDetails? organisation)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var httpContext = (context.Inner as HttpContext)!;

        string sid = GetSidValue(httpContext);
        string json = JsonSerializer.Serialize(organisation);

        // Update session values after having successfully retrieved both.
        httpContext.Session.SetString(AssociatedSidKey, sid);
        httpContext.Session.SetString(OrganisationKey, json);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task<ActiveOrganisationState?> GetActiveOrganisationStateAsync(IHttpContext context)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var httpContext = (context.Inner as HttpContext)!;

        try {
            string sid = GetSidValue(httpContext);
            if (httpContext.Session.GetString(AssociatedSidKey) != sid) {
                throw new MismatchedCallbackException("Mismatch with authenticated user session.");
            }

            string json = httpContext.Session.GetString(OrganisationKey);
            if (string.IsNullOrEmpty(json)) {
                throw new MismatchedCallbackException("Missing active organisation details.");
            }

            var organisation = JsonSerializer.Deserialize<OrganisationDetails>(json);
            // Note: The above may throw `JsonException`.

            return Task.FromResult<ActiveOrganisationState?>(
                new ActiveOrganisationState {
                    Organisation = organisation,
                }
            );
        }
        catch {
            httpContext.Session.Remove(AssociatedSidKey);
            httpContext.Session.Remove(OrganisationKey);
            return Task.FromResult<ActiveOrganisationState?>(null);
        }
    }
}
