using System.Net;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Search;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class CreateUserNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            CreateUserRequest,
            CreateUserNodeRequester
        >();
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(POST) http://directories.localhost/users"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "sub": "17a2a6cb-05a6-4603-b91e-06dde2a58a6a"
                }
                """),
        };
    }

    private static CreateUserNodeRequester CreateCreateUserNodeRequester(Dictionary<string, MappedResponse> responseMappings, AutoMocker autoMocker)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new CreateUserNodeRequester(directoriesClient, autoMocker.Get<IInteractionDispatcher>());
    }

    [TestMethod]
    public async Task MakesExpectedRequestToCreateUser()
    {
        var autoMocker = new AutoMocker();
        var interactor = CreateCreateUserNodeRequester(GetNodeResponseMappingsForHappyPath(), autoMocker);

        var response = await interactor.InvokeAsync(new CreateUserRequest {
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            EmailAddress = "jo.bradford@example.com",
            FirstName = "Jo",
            LastName = "Bradford",
        });

        Assert.IsNotNull(response);
        Assert.AreEqual(new Guid("17a2a6cb-05a6-4603-b91e-06dde2a58a6a"), response.UserId);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFailed()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://directories.localhost/users"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var autoMocker = new AutoMocker();
        var interactor = CreateCreateUserNodeRequester(responseMappings, autoMocker);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            interactor.InvokeAsync(new CreateUserRequest {
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
                EmailAddress = "jo.bradford@example.com",
                FirstName = "Jo",
                LastName = "Bradford",
            }));
    }

    [TestMethod]
    public async Task MakesExpectedRequestToUpdateSearchIndex()
    {
        var autoMocker = new AutoMocker();
        var interactor = CreateCreateUserNodeRequester(GetNodeResponseMappingsForHappyPath(), autoMocker);
        UpdateUserInSearchIndexRequest? capturedSearchIndexRequest = null;

        autoMocker.CaptureRequest<UpdateUserInSearchIndexRequest>(request => capturedSearchIndexRequest = request);

        await interactor.InvokeAsync(new CreateUserRequest {
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            EmailAddress = "jo.bradford@example.com",
            FirstName = "Jo",
            LastName = "Bradford",
        });

        Assert.IsNotNull(capturedSearchIndexRequest);
        Assert.AreEqual(Guid.Parse("17a2a6cb-05a6-4603-b91e-06dde2a58a6a"), capturedSearchIndexRequest.UserId);
    }
}
