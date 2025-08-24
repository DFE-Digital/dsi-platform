using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SupportTickets;
using Dfe.SignIn.Web.Help.Controllers;
using Dfe.SignIn.Web.Help.Models;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Controllers;

[TestClass]
public sealed class ContactControllerTests
{
    private static async Task AssertPresentsViewWithSubjectOptions(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction)
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest>(new GetSubjectOptionsForSupportTicketResponse {
            SubjectOptions = [
                new() { Code = "other", Description = "Other (please specify)" },
            ],
        });

        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);
        Assert.IsNotEmpty(viewModel.SubjectOptions);
        Assert.AreEqual("other", viewModel.SubjectOptions.First().Code);
        Assert.AreEqual("Other (please specify)", viewModel.SubjectOptions.First().Description);
    }

    private static async Task AssertPresentsViewWithApplicationOptions(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction)
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();

        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest>(new GetApplicationNamesForSupportTicketResponse {
            Applications = [
                new() { Name = "Service A" },
                new() { Name = "Service B" },
            ],
        });

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);
        CollectionAssert.AreEqual(
            new string[] { "Service A", "Service B" },
            viewModel.ApplicationOptions.Select(x => x.Name).ToArray()
        );
    }

    #region Index()

    [TestMethod]
    public Task Index_PresentsViewWithSubjectOptions()
    {
        return AssertPresentsViewWithSubjectOptions((autoMocker, controller) => controller.Index());
    }

    [TestMethod]
    public Task Index_PresentsViewWithApplicationOptions()
    {
        return AssertPresentsViewWithApplicationOptions((autoMocker, controller) => controller.Index());
    }

    #endregion

    #region PostIndex(ContactViewModel)

    [TestMethod]
    public async Task PostIndex_RaisesSupportTicketWithUserInputs()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();
        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        RaiseSupportTicketRequest? capturedRequest = null;
        autoMocker.CaptureRequest<RaiseSupportTicketRequest>(r => capturedRequest = r);

        var controller = autoMocker.CreateInstance<ContactController>();

        await controller.PostIndex(new ContactViewModel {
            FullName = "Alex Johnson",
            EmailAddress = "alex.johnson@example.com",
            SubjectCode = "other",
            CustomSummary = "Example custom summary.",
            OrganisationName = "Example Organisation",
            OrganisationURN = "123456",
            ApplicationName = "Example Service",
            Message = "Example message.",
        });

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("Alex Johnson", capturedRequest.FullName);
        Assert.AreEqual("alex.johnson@example.com", capturedRequest.EmailAddress);
        Assert.AreEqual("other", capturedRequest.SubjectCode);
        Assert.AreEqual("Example custom summary.", capturedRequest.CustomSummary);
        Assert.AreEqual("Example Organisation", capturedRequest.OrganisationName);
        Assert.AreEqual("123456", capturedRequest.OrganisationURN);
        Assert.AreEqual("Example Service", capturedRequest.ApplicationName);
        Assert.AreEqual("Example message.", capturedRequest.Message);
    }

    [TestMethod]
    public Task PostIndex_PresentsViewWithSubjectOptions_WhenRequestIsInvalid()
    {
        return AssertPresentsViewWithSubjectOptions((autoMocker, controller) => {
            autoMocker.MockThrows<RaiseSupportTicketRequest>(new InvalidRequestException());
            return controller.PostIndex(Activator.CreateInstance<ContactViewModel>());
        });
    }

    [TestMethod]
    public Task PostIndex_PresentsViewWithApplicationOptions_WhenRequestIsInvalid()
    {
        return AssertPresentsViewWithApplicationOptions((autoMocker, controller) => {
            autoMocker.MockThrows<RaiseSupportTicketRequest>(new InvalidRequestException());
            return controller.PostIndex(Activator.CreateInstance<ContactViewModel>());
        });
    }

    #endregion
}
