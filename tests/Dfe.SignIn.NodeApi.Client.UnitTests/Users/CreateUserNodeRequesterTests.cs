using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

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

            // Search API
            ["(POST) http://search.localhost/users/update-index"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    private static CreateUserNodeRequester CreateCreateUserNodeRequester(
        Dictionary<string, MappedResponse> responseMappings,
        List<string>? capturedLogs = null)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        var searchHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var searchClient = new HttpClient(searchHandlerMock.Object) {
            BaseAddress = new Uri("http://search.localhost")
        };

        capturedLogs ??= [];
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<CreateUserNodeRequester>(capturedLogs.Add);

        return new CreateUserNodeRequester(directoriesClient, searchClient, mockLogger.Object);
    }

    [TestMethod]
    public async Task MakesExpectedRequestToCreateUser()
    {
        var interactor = CreateCreateUserNodeRequester(GetNodeResponseMappingsForHappyPath());

        var response = await interactor.InvokeAsync(new CreateUserRequest {
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            EmailAddress = "jo.bradford@example.com",
            GivenName = "Jo",
            Surname = "Bradford",
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

        var interactor = CreateCreateUserNodeRequester(responseMappings);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            interactor.InvokeAsync(new CreateUserRequest {
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
                EmailAddress = "jo.bradford@example.com",
                GivenName = "Jo",
                Surname = "Bradford",
            }));
    }

    [TestMethod]
    public async Task MakesExpectedRequestToUpdateSearchIndex()
    {
        var capturedLogs = new List<string>();
        var interactor = CreateCreateUserNodeRequester(GetNodeResponseMappingsForHappyPath(), capturedLogs);

        await interactor.InvokeAsync(new CreateUserRequest {
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            EmailAddress = "jo.bradford@example.com",
            GivenName = "Jo",
            Surname = "Bradford",
        });

        Assert.Contains("Information: Updated search index for user '17a2a6cb-05a6-4603-b91e-06dde2a58a6a'.", capturedLogs);
    }

    [TestMethod]
    public async Task SucceedsButLogsError_WhenSearchIndexCouldNotBeUpdated()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://search.localhost/users/update-index"] =
                new MappedResponse(HttpStatusCode.InternalServerError, "null");

        var capturedLogs = new List<string>();
        var interactor = CreateCreateUserNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CreateUserRequest {
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            EmailAddress = "jo.bradford@example.com",
            GivenName = "Jo",
            Surname = "Bradford",
        });

        Assert.Contains("Error: Unable to update search index for user '17a2a6cb-05a6-4603-b91e-06dde2a58a6a'.", capturedLogs);
    }
}
