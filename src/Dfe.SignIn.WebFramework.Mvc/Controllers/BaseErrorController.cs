using System.Diagnostics.CodeAnalysis;
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
    public IActionResult Index([FromQuery] int code = 500)
    {
        this.Response.StatusCode = code;
        return code switch {
            404 => this.View("NotFound"),
            405 => HttpMethods.IsGet(this.Request.Method)
                ? this.Redirect("/") // eg. when authentication occurs when submitting a form.
                : this.ErrorView(),
            _ => this.ErrorView(),
        };
    }
}
