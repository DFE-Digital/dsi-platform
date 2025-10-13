using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class LinkEntraUserToDsiNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            LinkEntraUserToDsiRequest,
            LinkEntraUserToDsiNodeRequester
        >();
    }

    private static LinkEntraUserToDsiNodeRequester CreateLinkEntraUserToDsiNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new LinkEntraUserToDsiNodeRequester(directoriesClient);
    }

    [TestMethod]
    public async Task InvokeAsync_MakesExpectedRequestToNodeApi()
    {
        var interactor = CreateLinkEntraUserToDsiNodeRequester(new() {
            ["(POST) http://directories.localhost/users/17a2a6cb-05a6-4603-b91e-06dde2a58a6a/link-entra-oid"]
                = new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "sub": "17a2a6cb-05a6-4603-b91e-06dde2a58a6a"
                }
                """),
        });

        var response = await interactor.InvokeAsync(new LinkEntraUserToDsiRequest {
            DsiUserId = new Guid("17a2a6cb-05a6-4603-b91e-06dde2a58a6a"),
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            GivenName = "Jo",
            Surname = "Bradford",
        });

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenRequestWasNotSuccessful()
    {
        var interactor = CreateLinkEntraUserToDsiNodeRequester(new() {
            ["(POST) http://directories.localhost/users/17a2a6cb-05a6-4603-b91e-06dde2a58a6a/link-entra-oid"]
                = new MappedResponse(HttpStatusCode.InternalServerError)
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new LinkEntraUserToDsiRequest {
                DsiUserId = new Guid("17a2a6cb-05a6-4603-b91e-06dde2a58a6a"),
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
                GivenName = "Jo",
                Surname = "Bradford",
            }));
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenHasResponseMismatch()
    {
        var interactor = CreateLinkEntraUserToDsiNodeRequester(new() {
            ["(POST) http://directories.localhost/users/17a2a6cb-05a6-4603-b91e-06dde2a58a6a/link-entra-oid"]
                = new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "sub": "26a33b5b-cb6b-4a42-b260-ff8807566c4e"
                }
                """)
        });

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => interactor.InvokeAsync(new LinkEntraUserToDsiRequest {
                DsiUserId = new Guid("17a2a6cb-05a6-4603-b91e-06dde2a58a6a"),
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
                GivenName = "Jo",
                Surname = "Bradford",
            }));
        Assert.AreEqual("Response mismatch.", exception.Message);
    }
}
