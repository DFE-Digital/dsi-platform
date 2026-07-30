using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class GetUserProfileNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserProfileRequest,
            GetUserProfileNodeRequester
        >();
    }

    private static GetUserProfileNodeRequester CreateGetUserProfileNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new GetUserProfileNodeRequester(directoriesClient);
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(GET) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "isEntra": true,
                    "isInternalUser": true,
                    "given_name": "Bob",
                    "family_name": "Robinson",
                    "job_title": "Software Engineer",
                    "email": "bob.robinson@example.com"
                }
                """),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToGetUserProfile()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateGetUserProfileNodeRequester(responseMappings);

        var response = await interactor.InvokeAsync(new GetUserProfileRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
        });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsEntra);
        Assert.IsTrue(response.IsInternalUser);
        Assert.AreEqual("Bob", response.FirstName);
        Assert.AreEqual("Robinson", response.LastName);
        Assert.AreEqual("Software Engineer", response.JobTitle);
        Assert.AreEqual("bob.robinson@example.com", response.EmailAddress);
    }

    [TestMethod]
    public async Task Throws_WhenUserNotFound()
    {
        var interactor = CreateGetUserProfileNodeRequester(new() {
            ["(GET) http://directories.localhost/users/edd75704-0839-4f2a-be51-a6ecaf584019"] =
                new MappedResponse(HttpStatusCode.NotFound),
        });

        var exception = await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => interactor.InvokeAsync(new GetUserProfileRequest {
                UserId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            }));
        Assert.AreEqual(new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"), exception.UserId);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetUserProfileNodeRequester(new() {
            ["(GET) http://directories.localhost/users/edd75704-0839-4f2a-be51-a6ecaf584019"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetUserProfileRequest {
                UserId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            }));
    }
}
