using System.Diagnostics;
using Dfe.SignIn.SelectOrganisation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.SelectOrganisation.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this.logger = logger;
    }

    public IActionResult Index()
    {
        this.logger.LogTrace("Example log...");
        return this.View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel {
            RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier,
        });
    }
}
