using Dfe.SignIn.Core.Contracts.Notifications;
using Moq;
using Moq.AutoMock;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify.UnitTests;

[TestClass]
public sealed class SendEmailNotificationWithGovNotifyTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            SendEmailNotificationRequest,
            SendEmailNotificationWithGovNotifyUseCase
        >();
    }

    [TestMethod]
    public async Task SendsEmailWithExpectedParameters()
    {
        var autoMocker = new AutoMocker();

        var interactor = autoMocker.CreateInstance<SendEmailNotificationWithGovNotifyUseCase>();

        var request = new SendEmailNotificationRequest {
            RecipientEmailAddress = "alex.johnson@example.com",
            TemplateId = "template-id-123",
            Personalisation = [],
        };

        await interactor.InvokeAsync(request);

        autoMocker.Verify<IAsyncNotificationClient>(x => x.SendEmailAsync(
            It.Is<string>(emailAddress => emailAddress == request.RecipientEmailAddress),
            It.Is<string>(templateId => templateId == request.TemplateId),
            It.Is<Dictionary<string, dynamic>>(personalisation => ReferenceEquals(personalisation, request.Personalisation)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ));
    }

    [TestMethod]
    public async Task ReturnsValidResponse()
    {
        var autoMocker = new AutoMocker();

        var interactor = autoMocker.CreateInstance<SendEmailNotificationWithGovNotifyUseCase>();

        var request = new SendEmailNotificationRequest {
            RecipientEmailAddress = "alex.johnson@example.com",
            TemplateId = "template-id-123",
            Personalisation = [],
        };

        var response = await interactor.InvokeAsync(request);

        Assert.IsNotNull(response);
    }
}
