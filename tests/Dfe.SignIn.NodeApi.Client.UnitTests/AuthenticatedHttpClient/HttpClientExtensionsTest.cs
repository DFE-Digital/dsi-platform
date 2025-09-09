using System.Text;
using System.Net;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.AuthenticatedHttpClient;

[TestClass]
public sealed class HttpClientExtensionsTest
{
    private sealed record TestRequestModel
    {
        public required string Param1 { get; init; }

        public required int Param2 { get; init; }
    }

    private sealed record TestResponseModel
    {
        public required string Value { get; init; }
    }

    #region GetFromJsonOrDefaultAsync<TResponseBody>(HttpClient, string, CancellationToken)

    [TestMethod]
    public async Task GetFromJsonOrDefaultAsync_ReturnsDefault_WhenStatusCodeNotFound()
    {
        var handler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler);
        var result = await client.GetFromJsonOrDefaultAsync<TestResponseModel>("https://test.com");
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetFromJsonOrDefaultAsync_ReturnsParsedObject_WhenStatusIsSuccess()
    {
        string json = /*lang=json,strict*/ """
            { "Value": "test" }
        """;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var handler = new FakeHttpMessageHandler((req, ct) => Task.FromResult(responseMessage));
        var client = new HttpClient(handler);
        var result = await client.GetFromJsonOrDefaultAsync<TestResponseModel>("https://test.com");
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Value);
    }

    #endregion
}
