using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Controllers;

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
    [HttpGet]
    [SuppressMessage("csharpsquid", "S6967",
        Justification = "An error page should be presented regardless of ModelState."
    )]
    public IActionResult Index(int code = 500)
    {
        return code switch {
            404 => this.View("NotFound"),
            _ => this.ErrorView(),
        };
    }
}
