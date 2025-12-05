using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.Gateways.ServiceBus.Audit;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests.Audit;

[TestClass]
public sealed class WriteToAuditWithServiceBusTests
{
    private static WriteToAuditWithServiceBus CreateInteractor(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IAuditContextBuilder>()
            .Setup(x => x.BuildAuditContext())
            .Returns(new AuditContext {
                TraceId = "e3937d0e058449b6ae4a1438723c5b99",
                EnvironmentName = "test",
                SourceIpAddress = "127.0.0.1",
                SourceApplication = "FakeApplication",
                SourceUserId = new Guid("b1e26154-9b8a-4399-8662-5b4c1ffc01c3"),
            });

        return autoMocker.CreateInstance<WriteToAuditWithServiceBus>();
    }

    private static void CaptureServiceBusMessage(AutoMocker autoMocker, Action<ServiceBusMessage, CancellationToken> captureMessage)
    {
        autoMocker.GetMock<ServiceBusSender>()
            .Setup(x => x.SendMessageAsync(
                It.IsAny<ServiceBusMessage>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback(captureMessage);
    }

    private static string ApplyTripleDeserializationWorkaround(BinaryData json)
    {
        // Triple deserialize to workaround issue in existing system.
        string jsonWorkaround1 = JsonSerializer.Deserialize<string>(json)!;
        string[] jsonWorkaround2 = JsonSerializer.Deserialize<string[]>(jsonWorkaround1)!;
        return jsonWorkaround2[0];
    }

    [TestMethod]
    public async Task SendsExpectedMessage_WhenMinimumRequestSent()
    {
        var autoMocker = new AutoMocker();

        ServiceBusMessage? capturedMessage = null;
        CaptureServiceBusMessage(autoMocker, (message, _) => capturedMessage = message);

        var interactor = CreateInteractor(autoMocker);

        await interactor.InvokeAsync(new WriteToAuditRequest {
            EventCategory = "Login",
            Message = "Example audit message",
        }, CancellationToken.None);

        Assert.IsNotNull(capturedMessage);

        var body = JsonSerializer.Deserialize<JsonElement>(ApplyTripleDeserializationWorkaround(capturedMessage.Body));

        Assert.AreEqual("FakeApplication", body.GetProperty("application").GetString());
        Assert.AreEqual("b1e26154-9b8a-4399-8662-5b4c1ffc01c3", body.GetProperty("userId").GetString());
        Assert.AreEqual("Login", body.GetProperty("type").GetString());
        Assert.IsNull(body.GetProperty("subType").GetString());
        Assert.AreEqual("Example audit message", body.GetProperty("message").GetString());
        Assert.AreEqual("test", body.GetProperty("env").GetString());
        Assert.AreEqual("e3937d0e058449b6ae4a1438723c5b99", body.GetProperty("meta").GetProperty("req").GetString());
    }

    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public async Task SendsExpectedSuccessMetadata(bool wasFailure, bool expectedSuccessValue)
    {
        var autoMocker = new AutoMocker();

        ServiceBusMessage? capturedMessage = null;
        CaptureServiceBusMessage(autoMocker, (message, _) => capturedMessage = message);

        var interactor = CreateInteractor(autoMocker);

        await interactor.InvokeAsync(new WriteToAuditRequest {
            EventCategory = "Login",
            Message = "Example audit message",
            WasFailure = wasFailure,
        }, CancellationToken.None);

        Assert.IsNotNull(capturedMessage);

        var body = JsonSerializer.Deserialize<JsonElement>(ApplyTripleDeserializationWorkaround(capturedMessage.Body));

        Assert.AreEqual(expectedSuccessValue, body.GetProperty("meta").GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task SendsExpectedMessage_WhenFullRequestIsSent()
    {
        var autoMocker = new AutoMocker();

        ServiceBusMessage? capturedMessage = null;
        CaptureServiceBusMessage(autoMocker, (message, _) => capturedMessage = message);

        var interactor = CreateInteractor(autoMocker);

        await interactor.InvokeAsync(new WriteToAuditRequest {
            EventCategory = "Login",
            EventName = "Example",
            Message = "Example audit message",
            OrganisationId = new Guid("da7d2275-330d-4ba9-93a1-686162c994b0"),
            CustomProperties = [
                new("email", "jessica@example.com"),
            ],
        }, CancellationToken.None);

        Assert.IsNotNull(capturedMessage);

        var body = JsonSerializer.Deserialize<JsonElement>(ApplyTripleDeserializationWorkaround(capturedMessage.Body));

        Assert.AreEqual("FakeApplication", body.GetProperty("application").GetString());
        Assert.AreEqual("127.0.0.1", body.GetProperty("requestIp").GetString());
        Assert.AreEqual("b1e26154-9b8a-4399-8662-5b4c1ffc01c3", body.GetProperty("userId").GetString());
        Assert.AreEqual("da7d2275-330d-4ba9-93a1-686162c994b0", body.GetProperty("organisationid").GetString());
        Assert.AreEqual("Login", body.GetProperty("type").GetString());
        Assert.AreEqual("Example", body.GetProperty("subType").GetString());
        Assert.AreEqual("Example audit message", body.GetProperty("message").GetString());
        Assert.AreEqual("test", body.GetProperty("env").GetString());
        Assert.AreEqual("e3937d0e058449b6ae4a1438723c5b99", body.GetProperty("meta").GetProperty("req").GetString());
        Assert.IsTrue(body.GetProperty("meta").GetProperty("success").GetBoolean());
        Assert.AreEqual("jessica@example.com", body.GetProperty("meta").GetProperty("email").GetString());
    }

    [TestMethod]
    public async Task SendsExpectedMessage_WhenExplicitUserIdProvided()
    {
        var autoMocker = new AutoMocker();

        ServiceBusMessage? capturedMessage = null;
        CaptureServiceBusMessage(autoMocker, (message, _) => capturedMessage = message);

        var interactor = CreateInteractor(autoMocker);

        await interactor.InvokeAsync(new WriteToAuditRequest {
            EventCategory = "Login",
            Message = "Example audit message",
            UserId = new Guid("7290572c-efef-4f96-b814-735d1e1ad059"),
        }, CancellationToken.None);

        Assert.IsNotNull(capturedMessage);

        var body = JsonSerializer.Deserialize<JsonElement>(ApplyTripleDeserializationWorkaround(capturedMessage.Body));

        Assert.AreEqual("7290572c-efef-4f96-b814-735d1e1ad059", body.GetProperty("userId").GetString());
    }
}
