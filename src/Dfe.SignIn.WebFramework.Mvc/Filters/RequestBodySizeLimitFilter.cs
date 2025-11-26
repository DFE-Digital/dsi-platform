using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.SignIn.WebFramework.Mvc.Filters;

/// <summary>
/// A global resource filter that enforces the maximum request body size.
/// </summary>
public sealed class RequestBodySizeLimitFilter : IAsyncResourceFilter
{
    /// <summary>
    /// Executes during the resource filter phase, before model binding.
    /// Retrieves the maximum request body size for the endpoint, which may be configured via
    /// <see cref="Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute"/> or Kestrel server limits.
    /// </summary>
    public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;

        if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method)) {
            if (context.HttpContext.Response.StatusCode != StatusCodes.Status413RequestEntityTooLarge) {
                var maxRequestBodySizeFeature = context.HttpContext.Features.GetRequiredFeature<IHttpMaxRequestBodySizeFeature>();

                if (context.HttpContext.Request.ContentLength > maxRequestBodySizeFeature.MaxRequestBodySize) {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status413RequestEntityTooLarge;
                    context.ModelState.Clear();
                    return Task.CompletedTask;
                }
            }
        }
        return next();
    }
}
