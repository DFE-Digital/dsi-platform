using Azure.Core;
using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.InternalApi.Client.UnitTests.Fakes;
using Moq;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class AuthenticatedHttpClientHandlerTests
{
    private static HttpClient SetupHttpClient(
        HttpMessageHandler handler, AccessToken token = default, AuditContext? auditContext = null)
    {
        var mockAuditContextBuilder = new Mock<IAuditContextBuilder>();
        mockAuditContextBuilder
            .Setup(x => x.BuildAuditContext())
            .Returns(auditContext ?? new AuditContext {
                EnvironmentName = "test",
                SourceApplication = "Test",
                TraceId = "21e511b2cd9749198e65a81330a98ecb",
                SourceIpAddress = "127.0.0.1",
                SourceUserId = new Guid("b9e78942-5f84-44f1-b6e9-2553e36a0039"),
            });

        var mockCredential = new Mock<TokenCredential>();
        mockCredential
            .Setup(x => x.GetTokenAsync(
                It.IsAny<TokenRequestContext>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(token);

        string[] fakeScopes = ["example-scope/.default"];
        var authenticatedHttpClientHandler = new AuthenticatedHttpClientHandler(
            mockAuditContextBuilder.Object, mockCredential.Object, fakeScopes) {
            InnerHandler = handler
        };

        return new HttpClient(authenticatedHttpClientHandler);
    }

    #region SendAsync(HttpRequestMessage, CancellationToken)

    [TestMethod]
    public async Task SendAsync_AddsAuthorizationHeader()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var accessToken = new AccessToken("fake-bearer-token", new DateTimeOffset());
        var client = SetupHttpClient(testHandler, accessToken);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        await client.SendAsync(request);

        Assert.AreEqual("Bearer fake-bearer-token",
            testHandler.CapturedRequests[0].Headers.GetValues("Authorization").First());
    }

    [TestMethod]
    public async Task SendAsync_Throws_WhenStatusForbidden()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            return Task.FromResult(response);
        });

        var client = SetupHttpClient(testHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        var exception = await Assert.ThrowsExactlyAsync<HttpRequestException>(
            () => client.SendAsync(request)
        );
        Assert.AreEqual(HttpRequestError.ConnectionError, exception.HttpRequestError);
    }

    [TestMethod]
    public async Task SendAsync_AddsAuditHeaders()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var accessToken = new AccessToken("fake-bearer-token", new DateTimeOffset());
        var client = SetupHttpClient(testHandler, accessToken);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        await client.SendAsync(request);

#pragma warning disable MSTEST0037
        Assert.IsTrue(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceApplicationName));
        Assert.IsTrue(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceIpAddress));
        Assert.IsTrue(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceUserId));
#pragma warning restore MSTEST0037

        Assert.AreEqual("Test",
            testHandler.CapturedRequests[0].Headers.GetValues(AuditHeaderNames.SourceApplicationName).First());
        Assert.AreEqual("127.0.0.1",
            testHandler.CapturedRequests[0].Headers.GetValues(AuditHeaderNames.SourceIpAddress).First());
        Assert.AreEqual("b9e78942-5f84-44f1-b6e9-2553e36a0039",
            testHandler.CapturedRequests[0].Headers.GetValues(AuditHeaderNames.SourceUserId).First());
    }

    [TestMethod]
    public async Task SendAsync_OmitsMissingAuditHeaders()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var accessToken = new AccessToken("fake-bearer-token", new DateTimeOffset());

        var auditContext = new AuditContext {
            EnvironmentName = "test",
            SourceApplication = null!,
            TraceId = "21e511b2cd9749198e65a81330a98ecb",
            SourceIpAddress = null,
            SourceUserId = null,
        };

        var client = SetupHttpClient(testHandler, accessToken, auditContext);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        await client.SendAsync(request);

#pragma warning disable MSTEST0037
        Assert.IsFalse(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceApplicationName));
        Assert.IsFalse(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceIpAddress));
        Assert.IsFalse(testHandler.CapturedRequests[0].Headers.Contains(AuditHeaderNames.SourceUserId));
#pragma warning restore MSTEST0037
    }

    #endregion
}
