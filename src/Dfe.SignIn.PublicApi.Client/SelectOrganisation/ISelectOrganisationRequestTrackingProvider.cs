using Dfe.SignIn.PublicApi.Client.Abstractions;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service that tracks the active "select organisation" request.
/// </summary>
/// <remarks>
///   <para>This can help with the following scenarios:</para>
///   <list type="bullet">
///     <item>If a user intiates multiple requests to select an organisation across
///     multiple tabs within their web browser; only the most recent request will be
///     fulfilled.</item>
///     <item>Avoid replaying "select organisation" callbacks since tracking should be
///     cleared when a callback is being processed.</item>
///     <item>Protect against malicious actors redirecting a user to the callback since
///     this would be an unexpected behaviour.</item>
///   </list>
/// </remarks>
public interface ISelectOrganisationRequestTrackingProvider
{
    /// <summary>
    /// Sets the unique ID of the tracked "select organisation" request.
    /// </summary>
    /// <remarks>
    ///   <para>Tracking can be cleared by specifying a value of <c>null</c> for the
    ///   <paramref name="requestId"/> parameter.</para>
    /// </remarks>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestId">A unique value identifying the request.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    Task SetTrackedRequestAsync(IHttpContext context, Guid? requestId);

    /// <summary>
    /// Determines whether the given "select organisation" request is valid for the user.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="requestId">A unique value identifying a request.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the given "select organisation" request is
    ///   valid for the current user; otherwise, a value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    Task<bool> IsTrackingRequestAsync(IHttpContext context, Guid requestId);
}
