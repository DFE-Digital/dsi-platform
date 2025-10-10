using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Diagnostics;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class InternalApiRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            PingRequest,
            InternalApiRequester<PingRequest>
        >();
    }

    private static void SetupSerialization(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptionsMonitor<JsonSerializerOptions>>()
            .Setup(x => x.Get(
                It.Is<string>(name => name == JsonHelperExtensions.StandardOptionsKey)
            ))
            .Returns(JsonHelperExtensions.CreateStandardOptionsTestHelper());

        var jsonOptionsAccessor = autoMocker.GetMock<IOptionsMonitor<JsonSerializerOptions>>().Object;
        autoMocker.Use<IExceptionJsonSerializer>(new DefaultExceptionJsonSerializer(jsonOptionsAccessor));
    }

    private static HttpClient CreateMockHttpClient(Dictionary<string, MappedResponse> responseMappings)
    {
        var httpHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        return new HttpClient(httpHandlerMock.Object) {
            BaseAddress = new Uri("http://internal-api.localhost")
        };
    }

    [TestMethod]
    public async Task InitiatesExpectedRequestUsingHttpClient()
    {
        var fakeRequest = new PingRequest { Value = 123 };

        var httpClient = CreateMockHttpClient(new() {
            ["(POST) http://internal-api.localhost/interaction/Diagnostics.Ping"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "content": {
                        "type": "Dfe.SignIn.Core.Contracts.Diagnostics.PingResponse",
                        "data": {
                          "value": 123
                        }
                    },
                    "exception": null
                }
                """)
        });

        var autoMocker = new AutoMocker();
        SetupSerialization(autoMocker);
        autoMocker.Use(httpClient);

        var apiRequester = autoMocker.CreateInstance<InternalApiRequester<PingRequest>>();

        var response = await apiRequester.InvokeAsync(fakeRequest, CancellationToken.None);

        var pingResponse = TypeAssert.IsType<PingResponse>(response);
        Assert.AreEqual(123, pingResponse.Value);
    }

    [TestMethod]
    public async Task Throws_WhenExceptionOccurs()
    {
        var fakeRequest = new PingRequest { Value = 123 };
        var interactionContext = new InteractionContext<PingRequest>(fakeRequest);

        var httpClient = CreateMockHttpClient(new() {
            ["(POST) http://internal-api.localhost/interaction/Diagnostics.Ping"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "content": null,
                    "exception": {
                        "type": "Dfe.SignIn.Base.Framework.InvalidRequestException",
                        "data": {
                          "invocationId": "<invocationId>"
                        }
                    }
                }
                """.Replace("<invocationId>", interactionContext.InvocationId.ToString()))
        });

        var autoMocker = new AutoMocker();
        SetupSerialization(autoMocker);
        autoMocker.Use(httpClient);

        var apiRequester = autoMocker.CreateInstance<InternalApiRequester<PingRequest>>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => apiRequester.InvokeAsync(interactionContext, CancellationToken.None));
        Assert.AreEqual(interactionContext.InvocationId, exception.InvocationId);
    }

    [TestMethod]
    public async Task Throws_WhenHttpStatusIsNotSuccess()
    {
        var fakeRequest = new PingRequest { Value = 123 };
        var interactionContext = new InteractionContext<PingRequest>(fakeRequest);

        var httpClient = CreateMockHttpClient(new() {
            ["(POST) http://internal-api.localhost/interaction/Diagnostics.Ping"] =
                new MappedResponse(HttpStatusCode.InternalServerError)
        });

        var autoMocker = new AutoMocker();
        SetupSerialization(autoMocker);
        autoMocker.Use(httpClient);

        var apiRequester = autoMocker.CreateInstance<InternalApiRequester<PingRequest>>();

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => apiRequester.InvokeAsync(interactionContext, CancellationToken.None));
    }
}
