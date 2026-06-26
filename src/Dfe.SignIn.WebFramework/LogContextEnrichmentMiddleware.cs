using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Middleware that enriches the logging scope with request context:
/// the client-provided <c>x-correlation-id</c> header, and the authenticated user ID
/// (for web applications using OIDC).
/// </summary>
/// <remarks>
/// <para>
/// OpenTelemetry already handles its own TraceId/SpanId propagation automatically
/// (visible as <c>operation_Id</c> in App Insights). This middleware only enriches the
/// log scope with the additional request-level context described below.
/// </para>
/// <para>
/// The following structured properties are conditionally added to the logging scope:
/// <list type="bullet">
///   <item>
///     <description>
///       <c>ClientCorrelationId</c> — the value of the <c>x-correlation-id</c> request header,
///       when present. Allows API consumers to correlate their own request IDs with server logs.
///     </description>
///   </item>
///   <item>
///     <description>
///       <c>UserId</c> — the authenticated user's ID from the <c>ClaimTypes.NameIdentifier</c>
///       claim, when the request is from an authenticated OIDC user (web applications).
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <b>Data Protection note:</b> <c>UserId</c> values are persistent pseudonymous identifiers
/// and should be considered personal data. Ensure that access to log storage (e.g. Azure Monitor
/// / Log Analytics workspaces) is restricted to authorised operations staff, and that your
/// Data Protection Impact Assessment (DPIA) covers the logging of user identifiers.
/// </para>
/// </remarks>
public class LogContextEnrichmentMiddleware(RequestDelegate next, ILogger<LogContextEnrichmentMiddleware> logger)
{
    /// <summary>
    /// Processes the HTTP request and enriches the logging scope with available request context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var scope = new Dictionary<string, object>();

        // x-correlation-id header (client-provided, optional)
        var clientCorrelationId = context.Request.Headers["x-correlation-id"].FirstOrDefault();
        if (clientCorrelationId is not null) {
            scope["ClientCorrelationId"] = clientCorrelationId;
        }

        // Authenticated user context (web apps using OIDC authentication)
        var userClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim is not null) {
            scope["UserId"] = userClaim.Value;
        }

        // Custom enrichment hook for subclasses
        await EnrichScopeAsync(context, scope);

        if (scope.Count > 0) {
            using (logger.BeginScope(scope)) {
                await next(context);
            }
        }
        else {
            await next(context);
        }
    }

    /// <summary>
    /// Virtual hook to allow subclasses (such as in Public API) to enrich the logging scope
    /// with additional contextual values (e.g. Client ID).
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="scope">The dictionary containing scope keys and values.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task EnrichScopeAsync(HttpContext context, IDictionary<string, object> scope)
    {
        return Task.CompletedTask;
    }
}
