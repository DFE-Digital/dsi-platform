using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class GetUserStatusNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserStatusRequest,
            GetUserStatusNodeRequester
        >();
    }

    private static GetUserStatusNodeRequester CreateGetUserStatusNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new GetUserStatusNodeRequester(directoriesClient);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedUserStatus_WhenEntraUserIdIsProvided()
    {
        var interactor = CreateGetUserStatusNodeRequester(new() {
            ["(GET) http://directories.localhost/users/by-entra-oid/d9079a36-ce78-4265-afa3-1c0751e42616"]
                = new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "sub": "c97d2fc4-6951-487d-bbcd-87182e70ddce",
                    "status": 1
                }
                """),
        });

        var response = await interactor.InvokeAsync(new GetUserStatusRequest {
            EntraUserId = new Guid("d9079a36-ce78-4265-afa3-1c0751e42616"),
        });

        Assert.IsTrue(response.UserExists);
        Assert.AreEqual(new Guid("c97d2fc4-6951-487d-bbcd-87182e70ddce"), response.UserId);
        Assert.AreEqual(AccountStatus.Active, response.AccountStatus);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsDoesNotExist_WhenEntraUserIdIsProvided()
    {
        var interactor = CreateGetUserStatusNodeRequester(new() {
            ["(GET) http://directories.localhost/users/by-entra-oid/d9079a36-ce78-4265-afa3-1c0751e42616"]
                = new MappedResponse(HttpStatusCode.NotFound),
        });

        var response = await interactor.InvokeAsync(new GetUserStatusRequest {
            EntraUserId = new Guid("d9079a36-ce78-4265-afa3-1c0751e42616"),
        });

        Assert.IsFalse(response.UserExists);
        Assert.IsNull(response.UserId);
        Assert.IsNull(response.AccountStatus);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedUserStatus_WhenEmailAddressIsProvided()
    {
        var interactor = CreateGetUserStatusNodeRequester(new() {
            ["(GET) http://directories.localhost/users/jo.bradford@example.com"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "sub": "c97d2fc4-6951-487d-bbcd-87182e70ddce",
                    "status": 1
                }
                """)
        });

        var response = await interactor.InvokeAsync(new GetUserStatusRequest {
            EmailAddress = "jo.bradford@example.com",
        });

        Assert.IsTrue(response.UserExists);
        Assert.AreEqual(new Guid("c97d2fc4-6951-487d-bbcd-87182e70ddce"), response.UserId);
        Assert.AreEqual(AccountStatus.Active, response.AccountStatus);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsDoesNotExist_WhenEmailAddressIsProvided()
    {
        var interactor = CreateGetUserStatusNodeRequester(new() {
            ["(GET) http://directories.localhost/users/jo.bradford@example.com"] =
                new MappedResponse(HttpStatusCode.NotFound),
        });

        var response = await interactor.InvokeAsync(new GetUserStatusRequest {
            EmailAddress = "jo.bradford@example.com",
        });

        Assert.IsFalse(response.UserExists);
        Assert.IsNull(response.UserId);
        Assert.IsNull(response.AccountStatus);
    }
}
