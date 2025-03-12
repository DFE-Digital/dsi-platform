using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApiClient.UnitTests.AuthenticatedHttpClient;

[TestClass]
public class AuthenticatedHttpClientHandlerTests
{
    [TestMethod]
    public async Task AuthenticatedHttpClientHandler_ShouldHaveBearerTokenAddedToRequest()
    {

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var securityProvider = new FakeHttpSecurityProvider();
        var authenticatedHttpClientHandler = new AuthenticatedHttpClientHandler(securityProvider) {
            InnerHandler = testHandler
        };

        var client = new HttpClient(authenticatedHttpClientHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");
        await client.SendAsync(request);

        Assert.IsNotNull(testHandler.CapturedRequests);
        Assert.IsTrue(testHandler.CapturedRequests.First().Headers.Contains("Authorization"));

        string actualValue = testHandler.CapturedRequests.First().Headers.GetValues("Authorization").First();
        Assert.AreEqual("Bearer fake-bearer-token", actualValue);
    }
}
