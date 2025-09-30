using Azure.Core;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;
using Moq;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.AuthenticatedHttpClient;

[TestClass]
public sealed class AuthenticatedHttpClientHandlerTests
{
    #region SendAsync(HttpRequestMessage, CancellationToken)

    [TestMethod]
    public async Task SendAsync_ShouldHaveBearerTokenAddedToRequest()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var mockCredential = new Mock<TokenCredential>();
        mockCredential
            .Setup(x => x.GetTokenAsync(
                It.IsAny<TokenRequestContext>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(
                new AccessToken("fake-bearer-token", new DateTimeOffset())
            );

        string[] fakeScopes = ["example-scope/.default"];
        var authenticatedHttpClientHandler = new AuthenticatedHttpClientHandler(mockCredential.Object, fakeScopes) {
            InnerHandler = testHandler
        };

        var client = new HttpClient(authenticatedHttpClientHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");
        await client.SendAsync(request);

        Assert.IsTrue(testHandler.CapturedRequests[0].Headers.Contains("Authorization"));

        string actualValue = testHandler.CapturedRequests[0].Headers.GetValues("Authorization").First();
        Assert.AreEqual("Bearer fake-bearer-token", actualValue);
    }

    [TestMethod]
    public async Task SendAsync_Throws_WhenStatusForbidden()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            return Task.FromResult(response);
        });

        var mockCredential = new Mock<TokenCredential>();
        string[] fakeScopes = ["example-scope/.default"];
        var authenticatedHttpClientHandler = new AuthenticatedHttpClientHandler(mockCredential.Object, fakeScopes) {
            InnerHandler = testHandler
        };

        var client = new HttpClient(authenticatedHttpClientHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        var exception = await Assert.ThrowsExactlyAsync<HttpRequestException>(
            () => client.SendAsync(request)
        );
        Assert.AreEqual(HttpRequestError.ConnectionError, exception.HttpRequestError);
    }

    #endregion
}
