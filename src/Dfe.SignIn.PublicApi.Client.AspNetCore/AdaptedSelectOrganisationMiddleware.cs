using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Adapts the <see cref="StandardSelectOrganisationMiddleware"/> middleware to work
/// with ASP.NET Core middleware mechanism.
/// </summary>
public sealed class AdaptedSelectOrganisationMiddleware(IHttpMiddleware inner) : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var adapter = new HttpContextAspNetCoreAdapter(context);
        return inner.InvokeAsync(adapter, () => next(context));
    }
}
