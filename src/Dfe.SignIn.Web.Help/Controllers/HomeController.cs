using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for the home page of the help component.
/// </summary>
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return this.View();
    }
}
