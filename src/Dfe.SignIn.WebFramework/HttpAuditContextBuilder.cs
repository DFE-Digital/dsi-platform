using System.Diagnostics;
using System.Security.Claims;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Builds an <see cref="AuditContext"/> from the current HTTP context.
/// </summary>
public sealed class HttpAuditContextBuilder(
    IOptions<AuditOptions> optionsAccessor,
    IHttpContextAccessor httpContextAccessor
) : IAuditContextBuilder
{
    /// <inheritdoc/>
    public AuditContext BuildAuditContext()
    {
        var options = optionsAccessor.Value;

        string? sourceApplication = null;
        string? sourceIpAddress = null;
        string? sourceUserId = null;

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null) {
            sourceApplication = httpContext.Request.Headers[AuditHeaderNames.SourceApplicationName].FirstOrDefault();
            sourceIpAddress = httpContext.Request.Headers[AuditHeaderNames.SourceIpAddress].FirstOrDefault()
                ?? httpContext.Connection.RemoteIpAddress?.ToString();
            sourceUserId = httpContext.Request.Headers[AuditHeaderNames.SourceUserId].FirstOrDefault()
                ?? httpContext.User.FindFirstValue("dsi_user_id")
                ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        return new AuditContext {
            TraceId = Activity.Current?.TraceId.ToString()!,
            EnvironmentName = options.EnvironmentName,
            SourceApplication = sourceApplication ?? options.ApplicationName,
            SourceIpAddress = sourceIpAddress,
            SourceUserId = !string.IsNullOrWhiteSpace(sourceUserId) ? new Guid(sourceUserId) : null,
        };
    }
}
