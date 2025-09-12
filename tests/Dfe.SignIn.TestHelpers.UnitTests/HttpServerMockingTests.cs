using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class HttpServerMockingTests
{
    #region CreateJsonRequest<TBody>(TBody)

    private sealed record ExampleRequestBody
    {
        [JsonPropertyName("value")]
        public required int Value { get; init; }
    }

    [TestMethod]
    public async Task CreateJsonRequest_CreatesRequestWithExpectedData()
    {
        var fakeRequest = HttpServerMocking.CreateJsonRequest(new ExampleRequestBody {
            Value = 123,
        });

        var body = await fakeRequest.ReadFromJsonAsync<ExampleRequestBody?>();
        Assert.AreEqual(123, body!.Value);
    }

    #endregion
}
