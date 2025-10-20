using System.Net;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class CancelPendingChangeEmailAddressNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            CancelPendingChangeEmailAddressRequest,
            CancelPendingChangeEmailAddressNodeRequester
        >();
    }

    private static CancelPendingChangeEmailAddressNodeRequester CreateCancelPendingChangeEmailAddressNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        autoMocker.MockResponse(
            new GetUserProfileRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            },
            new GetUserProfileResponse {
                IsEntra = false,
                IsInternalUser = false,
                GivenName = "Alex",
                Surname = "Johnson",
                JobTitle = "Software Engineer",
                EmailAddress = "alex.johnson@example.com",
            }
        );

        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new CancelPendingChangeEmailAddressNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>()
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task WritesToAudit()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateCancelPendingChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        await interactor.InvokeAsync(new CancelPendingChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, capturedWriteToAudit.EventCategory);
        Assert.AreEqual(AuditChangeEmailEventNames.CancelChangeEmail, capturedWriteToAudit.EventName);
        Assert.AreEqual("Cancel change email request from alex.johnson@example.com (id: 51a50a75-e4fa-4b6e-9c72-581538ee5258)", capturedWriteToAudit.Message);
    }

    [TestMethod]
    public async Task MakesExpectedRequestToCancelPendingChangeEmailAddress()
    {
        var autoMocker = new AutoMocker();
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateCancelPendingChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        var response = await interactor.InvokeAsync(new CancelPendingChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
        });

        Assert.IsNotNull(response);

        var mapping = responseMappings["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"];
        Assert.HasCount(1, mapping.Invocations);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateCancelPendingChangeEmailAddressNodeRequester(new AutoMocker(), new() {
            ["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"] =
                new MappedResponse(HttpStatusCode.BadRequest),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new CancelPendingChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            }));
    }
}
