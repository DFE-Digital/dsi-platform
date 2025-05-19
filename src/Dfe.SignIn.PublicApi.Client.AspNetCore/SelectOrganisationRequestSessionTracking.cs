using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// An implementation of <see cref="ISelectOrganisationRequestTrackingProvider"/> which
/// tracks the active request to select an organisation in the user session.
/// </summary>
public sealed class SelectOrganisationRequestSessionTracking : ISelectOrganisationRequestTrackingProvider
{
    internal const string TrackedRequestIdSessionKey = "82471f22-923b-42b2-b518-91cf80f2af6d";

    /// <inheritdoc/>
    public Task SetTrackedRequestAsync(IHttpContext context, Guid? requestId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var httpContext = (context.Inner as HttpContext)!;

        if (requestId is not null) {
            httpContext.Session.SetString(TrackedRequestIdSessionKey, requestId.ToString());
        }
        else {
            httpContext.Session.Remove(TrackedRequestIdSessionKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsTrackingRequestAsync(IHttpContext context, Guid requestId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var httpContext = (context.Inner as HttpContext)!;

        string trackedRequestIdRaw = httpContext.Session.GetString(TrackedRequestIdSessionKey);
        Guid.TryParse(trackedRequestIdRaw, out var trackedRequestId);

        return Task.FromResult(trackedRequestId == requestId);
    }
}
