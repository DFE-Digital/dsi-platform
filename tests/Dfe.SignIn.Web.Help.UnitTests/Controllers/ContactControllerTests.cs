using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Controllers;
using Dfe.SignIn.Web.Help.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Controllers;

[TestClass]
public sealed class ContactControllerTests
{
    private static readonly TopicModel FakeContactUsTopic = new() {
        Path = "/contact-us",
        Metadata = new() {
            Title = "Longer contact us title",
            NavigationTitle = "Contact us",
        },
        ContentHtml = "<p>Leading paragraph...</p>",
    };

    private static void SetupMockContactUsTopic(AutoMocker autoMocker)
    {
        var mockTopicIndex = autoMocker.GetMock<ITopicIndex>();
        mockTopicIndex
            .Setup(x => x.GetTopic(It.Is<string>(topicPath => topicPath == "/contact-us")))
            .Returns(FakeContactUsTopic);

        autoMocker.GetMock<ITopicIndexAccessor>()
            .Setup(x => x.GetIndexAsync(It.IsAny<bool>()))
            .ReturnsAsync(mockTopicIndex.Object);
    }

    private static async Task AssertPresentsViewWithTopicContent(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction)
    {
        var autoMocker = new AutoMocker();
        SetupMockContactUsTopic(autoMocker);

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();
        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);
        Assert.AreEqual(FakeContactUsTopic.Metadata.Title, viewModel.Title);
        Assert.AreEqual(FakeContactUsTopic.Metadata.Summary, viewModel.Summary);
        Assert.AreEqual(FakeContactUsTopic.ContentHtml, viewModel.ContentHtml);
    }

    private static async Task AssertPresentsViewWithSubjectOptions(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction)
    {
        var autoMocker = new AutoMocker();
        SetupMockContactUsTopic(autoMocker);

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest>(
            new GetSubjectOptionsForSupportTicketResponse {
                SubjectOptions = [
                    new() { Code = "other", Description = "Other (please specify)" },
                ],
            }
        );

        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);
        Assert.IsNotEmpty(viewModel.SubjectOptions);
        Assert.AreEqual("other", viewModel.SubjectOptions.First().Code);
        Assert.AreEqual("Other (please specify)", viewModel.SubjectOptions.First().Description);
    }

    private static async Task AssertPresentsViewWithTraceOptions(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction, string? expectedExceptionTraceId = null)
    {
        var autoMocker = new AutoMocker();
        SetupMockContactUsTopic(autoMocker);

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();
        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);

        Assert.AreEqual(expectedExceptionTraceId, viewModel.ExceptionTraceId);
    }

    private static async Task AssertPresentsViewWithApplicationOptions(Func<AutoMocker, ContactController, Task<IActionResult>> invokeAction)
    {
        var autoMocker = new AutoMocker();
        SetupMockContactUsTopic(autoMocker);

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();

        autoMocker.MockResponse<GetApplicationNamesForSupportTicketRequest>(
            new GetApplicationNamesForSupportTicketResponse {
                Applications = [
                    new() { Name = "Service A" },
                    new() { Name = "Service B" },
                ],
            }
        );

        var controller = autoMocker.CreateInstance<ContactController>();

        var result = await invokeAction(autoMocker, controller);

        var viewModel = TypeAssert.IsViewModelType<ContactViewModel>(result);
        CollectionAssert.AreEqual(
            new string[] { "Service A", "Service B" },
            viewModel.ApplicationOptions.Select(x => x.Name).ToArray()
        );
    }

    private static async Task<(IActionResult Result, RaiseSupportTicketRequest? CapturedRequest)> PostIndexAndCaptureResultAsync(string? exceptionTraceId = null, string faxNumber = "123", string website = "")
    {
        var autoMocker = new AutoMocker();
        SetupMockContactUsTopic(autoMocker);

        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>();
        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();

        RaiseSupportTicketRequest? capturedRequest = null;
        autoMocker.CaptureRequest<RaiseSupportTicketRequest>(r => capturedRequest = r);

        var controller = autoMocker.CreateInstance<ContactController>();

        var vm = new ContactViewModel {
            Title = "Contact us",
            Summary = "Contact us if you need assistance.",
            ContentHtml = "<p>Contact us if you need assistance.</p>",

            SubjectOptions = [],
            ApplicationOptions = [],

            FullNameInput = "Alex Johnson",
            EmailAddressInput = "alex.johnson@example.com",
            SubjectCodeInput = "other",
            CustomSummaryInput = "Example custom summary.",
            OrganisationNameInput = "Example Organisation",
            OrganisationUrnInput = "123456",
            ApplicationNameInput = "Example Service",
            MessageInput = "Example message.",
            ExceptionTraceId = exceptionTraceId,

            FaxNumber = faxNumber,
            Website = website
        };

        var result = await controller.PostIndex(vm);

        return (result, capturedRequest);
    }

    #region Index()

    [TestMethod]
    public Task Index_PresentsViewWithTopicContent()
    {
        return AssertPresentsViewWithTopicContent((autoMocker, controller) => controller.Index());
    }

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

    [TestMethod]
    public Task Index_PresentsViewWithTraceOptions()
    {
        return AssertPresentsViewWithTraceOptions((autoMocker, controller) => controller.Index("fake-trace-id"), "fake-trace-id");
    }

    [TestMethod]
    public Task Index_PresentsViewWithNoTraceOptions()
    {
        return AssertPresentsViewWithTraceOptions((autoMocker, controller) => controller.Index());
    }

    #endregion

    #region PostIndex(ContactViewModel)

    [TestMethod]
    public async Task PostIndex_RaisesSupportTicketWithUserInputs()
    {
        var (_, capturedRequest) = await PostIndexAndCaptureResultAsync();

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("Alex Johnson", capturedRequest.FullName);
        Assert.AreEqual("alex.johnson@example.com", capturedRequest.EmailAddress);
        Assert.AreEqual("other", capturedRequest.SubjectCode);
        Assert.AreEqual("Example custom summary.", capturedRequest.CustomSummary);
        Assert.AreEqual("Example Organisation", capturedRequest.OrganisationName);
        Assert.AreEqual("123456", capturedRequest.OrganisationUrn);
        Assert.AreEqual("Example Service", capturedRequest.ApplicationName);
        Assert.AreEqual("Example message.", capturedRequest.Message);
        Assert.IsNull(capturedRequest.ExceptionTraceId);
    }

    [TestMethod]
    public async Task PostIndex_WithExceptionTraceId_RaisesSupportTicketWithUserInputs()
    {
        var (_, capturedRequest) = await PostIndexAndCaptureResultAsync(exceptionTraceId: "fake-trace-id");

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("Alex Johnson", capturedRequest.FullName);
        Assert.AreEqual("alex.johnson@example.com", capturedRequest.EmailAddress);
        Assert.AreEqual("other", capturedRequest.SubjectCode);
        Assert.AreEqual("Example custom summary.", capturedRequest.CustomSummary);
        Assert.AreEqual("Example Organisation", capturedRequest.OrganisationName);
        Assert.AreEqual("123456", capturedRequest.OrganisationUrn);
        Assert.AreEqual("Example Service", capturedRequest.ApplicationName);
        Assert.AreEqual("Example message.", capturedRequest.Message);
        Assert.AreEqual("fake-trace-id", capturedRequest.ExceptionTraceId);
    }

    [TestMethod]
    public async Task PostIndex_DoesNotRaiseSupportTicket_WhenFaxNumberIsIncorrect()
    {
        var (result, capturedRequest) = await PostIndexAndCaptureResultAsync(faxNumber: "bad-value");

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.AreEqual("Success", viewResult.ViewName);

        Assert.IsNull(capturedRequest);
    }

    [TestMethod]
    public async Task PostIndex_DoesNotRaiseSupportTicket_WhenWebsiteHoneypotIsPopulated()
    {
        var (result, capturedRequest) = await PostIndexAndCaptureResultAsync(website: "http://spam");

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.AreEqual("Success", viewResult.ViewName);

        Assert.IsNull(capturedRequest);
    }

    [TestMethod]
    public async Task PostIndex_RaisesSupportTicket_WhenHoneypotFieldsAreValid()
    {
        var (result, capturedRequest) = await PostIndexAndCaptureResultAsync();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.AreEqual("Success", viewResult.ViewName);

        Assert.IsNotNull(capturedRequest);
    }

    private static readonly Exception FakeInvalidRequestException = new InvalidRequestException(
        Guid.Empty,
        [new("Invalid input.", ["EmailAddress"])]
    );

    [TestMethod]
    public Task PostIndex_PresentsViewWithTopicContent_WhenRequestIsInvalid()
    {
        return AssertPresentsViewWithTopicContent((autoMocker, controller) => {
            autoMocker.MockThrows<RaiseSupportTicketRequest>(FakeInvalidRequestException);
            return controller.PostIndex(Activator.CreateInstance<ContactViewModel>());
        });
    }

    [TestMethod]
    public Task PostIndex_PresentsViewWithSubjectOptions_WhenRequestIsInvalid()
    {
        return AssertPresentsViewWithSubjectOptions((autoMocker, controller) => {
            autoMocker.MockThrows<RaiseSupportTicketRequest>(FakeInvalidRequestException);
            return controller.PostIndex(Activator.CreateInstance<ContactViewModel>());
        });
    }

    [TestMethod]
    public Task PostIndex_PresentsViewWithApplicationOptions_WhenRequestIsInvalid()
    {
        return AssertPresentsViewWithApplicationOptions((autoMocker, controller) => {
            autoMocker.MockThrows<RaiseSupportTicketRequest>(FakeInvalidRequestException);
            return controller.PostIndex(Activator.CreateInstance<ContactViewModel>());
        });
    }

    #endregion
}
