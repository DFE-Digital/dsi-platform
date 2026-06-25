using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Middleware that enriches the logging scope with the client-provided
/// correlation ID from the <c>x-correlation-id</c> request header.
/// </summary>
/// <remarks>
/// <para>
/// OpenTelemetry already handles its own TraceId/SpanId propagation automatically
/// (visible as <c>operation_Id</c> in App Insights). This middleware only deals with
/// the optional client-supplied correlation ID that API consumers may send.
/// </para>
/// <para>
/// When the header is present, <c>ClientCorrelationId</c> is added to the logging scope
/// so it appears as a structured property on every log entry for the request — no
/// endpoint code needs to extract or log it manually.
/// </para>
/// </remarks>
public class ClientCorrelationMiddleware(RequestDelegate next, ILogger<ClientCorrelationMiddleware> logger)
{
    /// <summary>
    /// Processes the HTTP request and enriches the logging scope with the client correlation ID if present.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientCorrelationId = context.Request.Headers["x-correlation-id"].FirstOrDefault();

        if (clientCorrelationId is not null) {
            using (logger.BeginScope(new Dictionary<string, object> {
                ["ClientCorrelationId"] = clientCorrelationId
            })) {
                await next(context);
            }
        }
        else {
            await next(context);
        }
    }
}
