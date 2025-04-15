using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Adapts middleware to work with ASP.NET Core.
/// </summary>
public sealed class HttpMiddlewareAspNetCoreAdapter(Func<IHttpMiddleware> middlewareFactory, RequestDelegate next)
{
    private readonly IHttpMiddleware inner = middlewareFactory();

    /// <inheritdoc />
    public Task InvokeAsync(HttpContext context)
    {
        var adapter = new HttpContextAspNetCoreAdapter(context);
        return this.inner.InvokeAsync(adapter, () => next(context));
    }
}
