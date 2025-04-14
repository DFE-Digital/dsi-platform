namespace Dfe.SignIn.PublicApi.Client.Abstractions;

/// <summary>
/// An abstraction representing some sort of HTTP request middleware.
/// </summary>
public interface IHttpMiddleware
{
    /// <summary>
    /// Called when the middleware is being used.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    /// <param name="next">Invokes the next middleware.</param>
    Task InvokeAsync(IHttpContext context, Func<Task> next);
}
