using System.Text.Json;
using Dfe.SignIn.PublicApi.Client.Users;
using Dfe.SignIn.PublicApi.Contracts.Users;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.Users;

[TestClass]
public sealed class QueryUserOrganisationApiRequesterTests
{
    #region InvokeAsync()

    [TestMethod]
    public async Task InvokeAsync_InvokesExpectedEndpoint()
    {
        Uri? actualUrl = null;
        var handlerMock = HttpClientMocking.CreateMockMessageHandlerWithJson(uri => actualUrl = uri,
            /*lang=json,strict*/ """
            {
                "userId": "74004f34-a6c3-4144-88a1-c32b1c2bd82b",
                "organisation": null
            }
            """
        );

        var mockClient = new Mock<IPublicApiClient>();
        mockClient
            .Setup(x => x.HttpClient)
            .Returns(new HttpClient(handlerMock.Object) {
                BaseAddress = new Uri("/"),
            });

        var instance = new QueryUserOrganisationApiRequester(
            mockClient.Object,
            JsonSerializerOptions.Web,
            "v2/users/{userId}/organisations/{organisationId}/query"
        );

        await instance.InvokeAsync(new QueryUserOrganisationApiRequest {
            UserId = new Guid("74004f34-a6c3-4144-88a1-c32b1c2bd82b"),
            OrganisationId = new Guid("3278635c-28df-415d-b715-96104733d931"),
        });

        var expectedUrl = new Uri("/v2/users/74004f34-a6c3-4144-88a1-c32b1c2bd82b/organisations/3278635c-28df-415d-b715-96104733d931/query");
        Assert.AreEqual(expectedUrl, actualUrl);
    }

    #endregion
}
