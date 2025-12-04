using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Models;
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
    private async Task<ContactViewModel> PrepareViewModel(
        ContactViewModel? viewModel = null,
        string? exceptionTraceId = null)
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
        viewModel.ExceptionTraceId = exceptionTraceId;

        return viewModel;
    }

    [HttpGet("contact-us")]
    public async Task<IActionResult> Index([FromQuery] string? exceptionTraceId = null)
    {
        return this.View(await this.PrepareViewModel(null, exceptionTraceId));
    }

    [HttpPost("contact-us")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(ContactViewModel viewModel)
    {
        if (viewModel.FaxNumber != ContactViewModel.HoneypotDefaultFaxValue || !string.IsNullOrEmpty(viewModel.Website)) {
            return this.View("Success");
        }

        await this.MapInteractionRequest<RaiseSupportTicketRequest>(viewModel)
            .InvokeAsync(interaction.DispatchAsync);

        if (!this.ModelState.IsValid) {
            return this.View("Index", await this.PrepareViewModel(viewModel));
        }

        return this.View("Success");
    }
}
