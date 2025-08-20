using Dfe.SignIn.Web.Help.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for the 'Contact us' page.
/// </summary>
public sealed class ContactController : Controller
{
    [HttpGet]
    [Route("/contact-us")]
    public async Task<IActionResult> Index()
    {
        return this.View(await this.PrepareViewModel());
    }

    [HttpPost]
    [Route("/contact-us")]
    public async Task<IActionResult> PostIndex(ContactViewModel viewModel)
    {
        if (!viewModel.SubjectOptions.Any(option => option.Value == viewModel.SelectedSubject)) {
            this.ModelState.AddModelError(nameof(ContactViewModel.SelectedSubject), "Invalid selection");
        }

        if (!this.ModelState.IsValid) {
            return this.View("Index", await this.PrepareViewModel(viewModel));
        }

        return this.View("Success");
    }

    private Task<ContactViewModel> PrepareViewModel(ContactViewModel? viewModel = null)
    {
        viewModel ??= Activator.CreateInstance<ContactViewModel>();

        viewModel.ServiceOptions = [
            "Fake Service A",
            "Other (please specify)",
            "None",
        ];

        return Task.FromResult(viewModel);
    }
}
