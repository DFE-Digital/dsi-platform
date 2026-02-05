using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.WebFramework.Mvc.Filters;

/// <summary>
/// A result filter that detects antiforgery validation failures
/// and logs them using <see cref="ILogger"/> for diagnostics.
/// </summary>
public class LogAntiforgeryFailureFilter : IAlwaysRunResultFilter
{
    private readonly ILogger<LogAntiforgeryFailureFilter> logger;

    /// <summary>
    /// Creates a new instance of <see cref="LogAntiforgeryFailureFilter"/> with a logger.
    /// </summary>
    /// <param name="logger">The logger used to record antiforgery failures.</param>
    public LogAntiforgeryFailureFilter(ILogger<LogAntiforgeryFailureFilter> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public void OnResultExecuted(ResultExecutedContext context)
    {
        if (context.Result is AntiforgeryValidationFailedResult) {
            this.logger.LogError("Antiforgery validation failed for request path: {Path}",
            context.HttpContext.Request.Path);
        }
    }

    /// <inheritdoc/>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // No-op. Method intentionally left empty.
    }
}
