using System.Text;
using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApiClient.UnitTests.AuthenticatedHttpClient;

[TestClass]
public class HttpClientExtensionsTest
{

    private record TestModel()
    {
        public required string Value { get; set; }
    }

    [TestMethod]
    public async Task GetFromJsonOrDefaultAsync_ReturnsDefault_WhenStatusCodeNotFound()
    {
        var handler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler);
        var result = await client.GetFromJsonOrDefaultAsync<TestModel>("https://test.com");
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetFromJsonOrDefaultAsync_ReturnsParsedObject_WhenStatusIsSuccess()
    {
        var json = /*lang=json,strict*/ """
            { "Value": "test" }
        """;

        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var handler = new FakeHttpMessageHandler((req, ct) => Task.FromResult(responseMessage));
        var client = new HttpClient(handler);
        var result = await client.GetFromJsonOrDefaultAsync<TestModel>("https://test.com");
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Value);
    }
}
