using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class GetPendingChangeEmailAddressNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetPendingChangeEmailAddressRequest,
            GetPendingChangeEmailAddressNodeRequester
        >();
    }

    private static GetPendingChangeEmailAddressNodeRequester CreateGetPendingChangeEmailAddressNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new GetPendingChangeEmailAddressNodeRequester(
            directoriesClient,
            new MockTimeProvider(new(2025, 11, 10, 11, 24, 0, TimeSpan.Zero))
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(GET) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "email": "bob.robinson@example.com",
                    "code": "ABC1234",
                    "createdAt": "2025-11-10T11:23:41"
                }
                """),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToGetPendingChangeEmailAddress()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateGetPendingChangeEmailAddressNodeRequester(responseMappings);

        var response = await interactor.InvokeAsync(new GetPendingChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.PendingChangeEmailAddress);
        Assert.AreEqual("bob.robinson@example.com", response.PendingChangeEmailAddress.NewEmailAddress);
        Assert.AreEqual("ABC1234", response.PendingChangeEmailAddress.VerificationCode);

        var expectedExpiry = new DateTime(2025, 11, 10, 12, 23, 41, DateTimeKind.Utc);
        Assert.AreEqual(expectedExpiry, response.PendingChangeEmailAddress.ExpiryTimeUtc);

        Assert.IsFalse(response.PendingChangeEmailAddress.HasExpired);
    }

    [TestMethod]
    public async Task MarksAsExpired_WhenPendingRequestHasExpired()
    {
        var interactor = CreateGetPendingChangeEmailAddressNodeRequester(new() {
            ["(GET) http://directories.localhost/usercodes/c173ec59-6670-4aca-b433-61c949a6f370/changeemail"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "email": "bob.robinson@example.com",
                    "code": "ABC1234",
                    "createdAt": "2024-11-10T11:23:41"
                }
                """),
        });

        var response = await interactor.InvokeAsync(new GetPendingChangeEmailAddressRequest {
            UserId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.PendingChangeEmailAddress);
        Assert.AreEqual("bob.robinson@example.com", response.PendingChangeEmailAddress.NewEmailAddress);
        Assert.AreEqual("ABC1234", response.PendingChangeEmailAddress.VerificationCode);

        var expectedExpiry = new DateTime(2024, 11, 10, 12, 23, 41, DateTimeKind.Utc);
        Assert.AreEqual(expectedExpiry, response.PendingChangeEmailAddress.ExpiryTimeUtc);

        Assert.IsTrue(response.PendingChangeEmailAddress.HasExpired);
    }

    [TestMethod]
    public async Task ReturnsNoPendingChange_WhenNotFound()
    {
        var interactor = CreateGetPendingChangeEmailAddressNodeRequester(new() {
            ["(GET) http://directories.localhost/usercodes/edd75704-0839-4f2a-be51-a6ecaf584019/changeemail"] =
                new MappedResponse(HttpStatusCode.NotFound),
        });

        var response = await interactor.InvokeAsync(new GetPendingChangeEmailAddressRequest {
            UserId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
        });

        Assert.IsNotNull(response);
        Assert.IsNull(response.PendingChangeEmailAddress);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetPendingChangeEmailAddressNodeRequester(new() {
            ["(GET) http://directories.localhost/usercodes/edd75704-0839-4f2a-be51-a6ecaf584019/changeemail"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            }));
    }
}
