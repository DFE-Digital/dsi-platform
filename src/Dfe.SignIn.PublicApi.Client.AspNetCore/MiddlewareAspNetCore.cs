using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Adapts middleware to work with ASP.NET Core.
/// </summary>
public sealed class HttpMiddlewareAspNetCoreAdapter(IHttpMiddleware inner, RequestDelegate next)
{
    /// <inheritdoc />
    public Task InvokeAsync(HttpContext context)
    {
        return inner.InvokeAsync(new HttpContextAspNetCoreAdapter(context), () => next(context));
    }
}
