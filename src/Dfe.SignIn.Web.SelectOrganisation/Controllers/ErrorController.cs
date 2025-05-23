using System.Diagnostics;
using Dfe.SignIn.Web.SelectOrganisation.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.SelectOrganisation.Controllers;

/// <summary>
/// A user facing controller to present a general error message.
/// </summary>
public sealed class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        return this.View(new ErrorViewModel {
            RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier,
        });
    }
}
