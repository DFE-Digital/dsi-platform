using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SupportTickets;

[TestClass]
public sealed class RaiseSupportTicketByEmailUseCaseTests
{
    private static readonly RaiseSupportTicketRequest FakeRequest = new() {
        FullName = "Alex Johnson",
        EmailAddress = "alex@example.com",
        OrganisationName = "Example Organisation",
        SubjectCode = "create-account",
        ApplicationName = "Example Service",
        Message = "A message.",
    };

    private static void SetupOptions(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptions<RaiseSupportTicketByEmailOptions>>()
            .Setup(x => x.Value)
            .Returns(new RaiseSupportTicketByEmailOptions {
                EmailTemplateId = "123",
                SupportEmailAddress = "support@example.com",
                ContactUrl = new Uri("https://help.localhost/contact-us"),
            });
    }

    private static void SetupSubjectOptionsResponse(AutoMocker autoMocker)
    {
        autoMocker.MockResponse<GetSubjectOptionsForSupportTicketRequest>(
            new GetSubjectOptionsForSupportTicketResponse {
                SubjectOptions = [
                    new() { Code = "create-account", Description = "Creating an account" },
                    new() { Code = "other", Description = "Other (please specify)" },
                ],
            }
        );
    }

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            RaiseSupportTicketRequest,
            RaiseSupportTicketByEmailUseCase
        >(autoMocker);
    }

    [TestMethod]
    public Task Throws_WhenInvalidSubjectCodeIsProvided()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        return Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(FakeRequest with {
                SubjectCode = "unexpected",
            }));
    }

    [TestMethod]
    public Task Throws_WhenRequiredConfigurationIsMissing()
    {
        var autoMocker = new AutoMocker();
        SetupSubjectOptionsResponse(autoMocker);

        autoMocker.GetMock<IOptions<RaiseSupportTicketByEmailOptions>>()
            .Setup(x => x.Value)
            .Returns(new RaiseSupportTicketByEmailOptions {
                EmailTemplateId = "123",
                SupportEmailAddress = "",
                ContactUrl = new Uri("https://help.localhost/contact-us"),
            });

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        return Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => interactor.InvokeAsync(FakeRequest));
    }

    [TestMethod]
    public async Task SendsEmailToExpectedRecipient()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        SendEmailNotificationRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SendEmailNotificationRequest>(r => capturedRequest = r);

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        await interactor.InvokeAsync(FakeRequest);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("support@example.com", capturedRequest.RecipientEmailAddress);
    }

    [TestMethod]
    public async Task SendsEmailWithExpectedTemplateId()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        SendEmailNotificationRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SendEmailNotificationRequest>(r => capturedRequest = r);

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        await interactor.InvokeAsync(FakeRequest);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("123", capturedRequest.TemplateId);
    }

    [TestMethod]
    public async Task SendsEmailWithExpectedPersonalisation()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        SendEmailNotificationRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SendEmailNotificationRequest>(r => capturedRequest = r);

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        await interactor.InvokeAsync(FakeRequest);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("Alex Johnson", capturedRequest.Personalisation["name"]);
        Assert.AreEqual("alex@example.com", capturedRequest.Personalisation["email"]);
        Assert.AreEqual("Example Organisation", capturedRequest.Personalisation["orgName"]);
        Assert.AreEqual("", capturedRequest.Personalisation["urn"]);
        Assert.AreEqual("create-account", capturedRequest.Personalisation["type"]);
        Assert.AreEqual("Example Service", capturedRequest.Personalisation["service"]);
        Assert.AreEqual("A message.", capturedRequest.Personalisation["message"]);
        Assert.IsFalse(capturedRequest.Personalisation["showAdditionalInfoHeader"]);
        Assert.AreEqual("https://help.localhost/contact-us", capturedRequest.Personalisation["helpUrl"].ToString());
    }

    [TestMethod]
    public async Task SendsEmailWithOtherSubjectCode()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupSubjectOptionsResponse(autoMocker);

        SendEmailNotificationRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SendEmailNotificationRequest>(r => capturedRequest = r);

        var interactor = autoMocker.CreateInstance<RaiseSupportTicketByEmailUseCase>();

        await interactor.InvokeAsync(FakeRequest with {
            SubjectCode = "other",
            CustomSummary = "Custom summary",
        });

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("other", capturedRequest.Personalisation["type"]);
        Assert.IsTrue(capturedRequest.Personalisation["showAdditionalInfoHeader"]);
        Assert.AreEqual("Custom summary", capturedRequest.Personalisation["typeAdditionalInfo"]);
    }
}
