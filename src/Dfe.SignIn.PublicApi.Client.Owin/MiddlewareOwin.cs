using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.Owin;

namespace Dfe.SignIn.PublicApi.Client.Owin;

/// <summary>
/// Adapts middleware to work with OWIN.
/// </summary>
public sealed class HttpMiddlewareOwinAdapter(IHttpMiddleware inner)
{
    /// <inheritdoc />
    public Task InvokeAsync(IOwinContext context, Func<Task> next)
    {
        return inner.InvokeAsync(new HttpContextOwinAdapter(context), next);
    }
}
