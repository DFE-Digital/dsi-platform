using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// A global exception handler that captures unhandled exceptions in the Internal API,
/// logs them, and returns a structured RFC 7807 ProblemDetails JSON response.
/// Includes the ambient Trace ID and client correlation ID (if present) for diagnostics.
/// </summary>
///
[ExcludeFromCodeCoverage(Justification = "We could come back and test this, but it is not worth the effort for now.")]
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

/// <summary>
/// Extension method to add the validation filter to the endpoint builder.
/// </summary>
public static class ExceptionHandlerExtensions
{
    /// <summary>
    /// Adds the global exception handler to the service collection.
    /// </summary>
    /// <param name="services"></param>
    public static void AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}
