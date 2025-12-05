using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class ChangeNameNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            ChangeNameRequest,
            ChangeNameNodeRequester
        >();
    }

    private static ChangeNameNodeRequester CreateChangeNameNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new ChangeNameNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>()
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToChangeName()
    {
        var autoMocker = new AutoMocker();
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateChangeNameNodeRequester(autoMocker, responseMappings);

        var response = await interactor.InvokeAsync(new ChangeNameRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            GivenName = "Bob",
            Surname = "Robinson",
        });

        Assert.IsNotNull(response);

        var mapping = responseMappings["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"];
        Assert.HasCount(1, mapping.Invocations);

        var body = JsonSerializer.Deserialize<JsonElement>(mapping.Invocations[0].Body!);
        Assert.AreEqual("Bob", body.GetProperty("given_name").GetString());
        Assert.AreEqual("Robinson", body.GetProperty("family_name").GetString());
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateChangeNameNodeRequester(new AutoMocker(), new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.BadRequest),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new ChangeNameRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                GivenName = "Bob",
                Surname = "Robinson",
            }));
    }

    [TestMethod]
    public async Task WritesToAudit_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateChangeNameNodeRequester(autoMocker, responseMappings);

        await interactor.InvokeAsync(new ChangeNameRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            GivenName = "Bob",
            Surname = "Robinson",
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeName, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Successfully changed name to Bob Robinson", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
        Assert.IsFalse(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task WritesToAudit_WhenFailed()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var interactor = CreateChangeNameNodeRequester(autoMocker, new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.BadRequest),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new ChangeNameRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                GivenName = "Bob",
                Surname = "Robinson",
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeName, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Failed to change name to Bob Robinson", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }
}
