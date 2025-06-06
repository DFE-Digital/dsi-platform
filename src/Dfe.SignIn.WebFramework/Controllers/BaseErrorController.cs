using System.Diagnostics;
using Dfe.SignIn.WebFramework.Models;
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
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        return this.View(new ErrorViewModel {
            RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier,
        });
    }
}
