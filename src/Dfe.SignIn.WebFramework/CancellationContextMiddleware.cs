using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// A middleware which updates the cancellation context from the current request.
/// </summary>
/// <remarks>
///   <para>This middleware must be registered as early as possible when setting
///   up the application. Ideally this should be the first middleware that is
///   registered to ensure that cancellation tokens are propagated correctly
///   throughout interactions.</para>
/// </remarks>
/// <param name="cancellationContext">The ambient cancellation context.</param>
/// <param name="next">The next delegate/middleware in the request pipeline.</param>
public sealed class CancellationContextMiddleware(
    ICancellationContext cancellationContext,
    RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware for the specified <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        cancellationContext.CancellationToken = context.RequestAborted;
        await next(context);
    }
}
