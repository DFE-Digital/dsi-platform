using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.InternalApi;

/// <summary>
/// A global exception handler that captures unhandled exceptions in the Internal API,
/// logs them, and returns a structured RFC 7807 ProblemDetails JSON response.
/// Includes the ambient Trace ID and client correlation ID (if present) for diagnostics.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred during request execution.");

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var correlationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        var problemDetails = new ProblemDetails {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred on the server.",
            Detail = "Please contact support referencing the trace identifiers.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        if (!string.IsNullOrEmpty(correlationId)) {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
