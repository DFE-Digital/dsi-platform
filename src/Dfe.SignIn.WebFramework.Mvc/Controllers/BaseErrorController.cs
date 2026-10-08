using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Mvc.Controllers;

/// <summary>
/// A user facing controller to present a general error message.
/// </summary>
/// <remarks>
///   <para>An application can extend this controller for the default behaviour.</para>
/// </remarks>
public abstract class BaseErrorController : Controller
{
    /// <summary>
    /// Presents the error page.
    /// </summary>
    /// <param name="code">HTTP status code.</param>
    [SuppressMessage("csharpsquid", "S6967",
        Justification = "An error page should be presented regardless of ModelState."
    )]
    [SuppressMessage("csharpsquid", "S5693",
        Justification = "Size validated upstream by RequestSizeLimit attribute, RequestBodySizeLimitFilter filter and Kestrel."
    )]
    [DisableRequestSizeLimit]
    public IActionResult Index([FromQuery] int code = StatusCodes.Status500InternalServerError)
    {
        return code switch {
            StatusCodes.Status404NotFound => this.ErrorView(FrameworkErrorPresets.NotFound()),
            StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
                => this.ErrorView(FrameworkErrorPresets.NotAuthorised(code)),
            StatusCodes.Status405MethodNotAllowed => HttpMethods.IsGet(this.Request.Method)
                ? this.Redirect("/") // eg. when authentication occurs when submitting a form.
                : this.ServerErrorView(code),
            _ => this.ServerErrorView(code),
        };
    }

    private IActionResult ServerErrorView(int statusCode)
    {
        var model = FrameworkErrorPresets.DefaultServerError(statusCode);
        model.RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
        return this.ErrorView(model);
    }
}
