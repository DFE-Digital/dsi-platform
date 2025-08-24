using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SupportTickets;
using Dfe.SignIn.Web.Help.Models;
using Dfe.SignIn.WebFramework;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for the 'Contact us' page.
/// </summary>
public sealed class ContactController(
    IInteractionDispatcher interaction
) : Controller
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
        try {
            await interaction.DispatchAsync(new RaiseSupportTicketRequest {
                FullName = viewModel.FullName!,
                EmailAddress = viewModel.EmailAddress!,
                SubjectCode = viewModel.SubjectCode!,
                CustomSummary = viewModel.CustomSummary,
                OrganisationName = viewModel.OrganisationName!,
                OrganisationURN = viewModel.OrganisationURN,
                ApplicationName = viewModel.ApplicationName!,
                Message = viewModel.Message!,
            }).To<RaiseSupportTicketResponse>();

            return this.View("Success");
        }
        catch (InvalidRequestException ex) {
            this.ModelState.AddFrom(ex.ValidationResults, new() {
                [nameof(RaiseSupportTicketRequest.FullName)] = nameof(ContactViewModel.FullName),
                [nameof(RaiseSupportTicketRequest.EmailAddress)] = nameof(ContactViewModel.EmailAddress),
                [nameof(RaiseSupportTicketRequest.SubjectCode)] = nameof(ContactViewModel.SubjectCode),
                [nameof(RaiseSupportTicketRequest.CustomSummary)] = nameof(ContactViewModel.CustomSummary),
                [nameof(RaiseSupportTicketRequest.OrganisationName)] = nameof(ContactViewModel.OrganisationName),
                [nameof(RaiseSupportTicketRequest.OrganisationURN)] = nameof(ContactViewModel.OrganisationURN),
                [nameof(RaiseSupportTicketRequest.ApplicationName)] = nameof(ContactViewModel.ApplicationName),
                [nameof(RaiseSupportTicketRequest.Message)] = nameof(ContactViewModel.Message),
            });
            return this.View("Index", await this.PrepareViewModel(viewModel));
        }
    }

    private async Task<ContactViewModel> PrepareViewModel(ContactViewModel? viewModel = null)
    {
        viewModel ??= Activator.CreateInstance<ContactViewModel>();

        var subjectOptionsResponse = await interaction.DispatchAsync(
            new GetSubjectOptionsForSupportTicketRequest()
        ).To<GetSubjectOptionsForSupportTicketResponse>();

        viewModel.SubjectOptions = subjectOptionsResponse.SubjectOptions;

        var applicationNamesResponse = await interaction.DispatchAsync(
            new GetApplicationNamesForSupportTicketRequest()
        ).To<GetApplicationNamesForSupportTicketResponse>();

        viewModel.ApplicationOptions = applicationNamesResponse.Applications;

        return viewModel;
    }
}
