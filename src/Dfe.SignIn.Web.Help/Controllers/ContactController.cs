using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SupportTickets;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Models;
using Dfe.SignIn.WebFramework;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for the 'Contact us' page.
/// </summary>
[Route("/")]
public sealed class ContactController(
    ITopicIndexAccessor topicIndexAccessor,
    IInteractionDispatcher interaction
) : Controller
{
    private async Task<ContactViewModel> PrepareViewModel(ContactViewModel? viewModel = null)
    {
        viewModel ??= Activator.CreateInstance<ContactViewModel>();

        var topicIndex = await topicIndexAccessor.GetIndexAsync();
        var topic = topicIndex.GetRequiredTopic("/contact-us");
        viewModel.Title = topic.Metadata.Title;
        viewModel.Summary = topic.Metadata.Summary;
        viewModel.ContentHtml = topic.ContentHtml;

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

    [HttpGet("contact-us")]
    public async Task<IActionResult> Index()
    {
        return this.View(await this.PrepareViewModel());
    }

    [HttpPost("contact-us")]
    public async Task<IActionResult> PostIndex(ContactViewModel viewModel)
    {
        try {
            await interaction.DispatchAsync(new RaiseSupportTicketRequest {
                FullName = viewModel.FullNameInput!,
                EmailAddress = viewModel.EmailAddressInput!,
                SubjectCode = viewModel.SubjectCodeInput!,
                CustomSummary = viewModel.CustomSummaryInput,
                OrganisationName = viewModel.OrganisationNameInput!,
                OrganisationURN = viewModel.OrganisationUrnInput,
                ApplicationName = viewModel.ApplicationNameInput!,
                Message = viewModel.MessageInput!,
            }).To<RaiseSupportTicketResponse>();
        }
        catch (InvalidRequestException ex) {
            this.ModelState.AddFrom(ex.ValidationResults, new() {
                [nameof(RaiseSupportTicketRequest.FullName)] = nameof(ContactViewModel.FullNameInput),
                [nameof(RaiseSupportTicketRequest.EmailAddress)] = nameof(ContactViewModel.EmailAddressInput),
                [nameof(RaiseSupportTicketRequest.SubjectCode)] = nameof(ContactViewModel.SubjectCodeInput),
                [nameof(RaiseSupportTicketRequest.CustomSummary)] = nameof(ContactViewModel.CustomSummaryInput),
                [nameof(RaiseSupportTicketRequest.OrganisationName)] = nameof(ContactViewModel.OrganisationNameInput),
                [nameof(RaiseSupportTicketRequest.OrganisationURN)] = nameof(ContactViewModel.OrganisationUrnInput),
                [nameof(RaiseSupportTicketRequest.ApplicationName)] = nameof(ContactViewModel.ApplicationNameInput),
                [nameof(RaiseSupportTicketRequest.Message)] = nameof(ContactViewModel.MessageInput),
            });
            this.ModelState.ThrowIfNoErrorsRecorded(ex);
        }

        if (!this.ModelState.IsValid) {
            return this.View("Index", await this.PrepareViewModel(viewModel));
        }

        return this.View("Success");
    }
}
