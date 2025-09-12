using System.Net;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Client.Internal;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Moq.Protected;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.Internal;

[TestClass]
public sealed class GetUserAccessToServiceApiRequesterTests
{
    private static void UseFakeOptions(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptions<PublicApiOptions>>()
            .Setup(x => x.Value)
            .Returns(new PublicApiOptions {
                ClientId = "<client_id>",
                ApiSecret = "<api_secret>",
            });

        autoMocker.Use(JsonHelperExtensions.CreateStandardOptionsTestHelper());
    }

    private static void UseHttpClient(AutoMocker autoMocker, HttpClient httpClient)
    {
        autoMocker.GetMock<IPublicApiClient>()
            .Setup(x => x.HttpClient)
            .Returns(httpClient);
    }

    private static void UseHttpClientWithFakeResponse(AutoMocker autoMocker, string fakeHttpResponse)
    {
        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeHttpResponse),
            });

        UseHttpClient(autoMocker, new HttpClient(mockHttp.Object) {
            BaseAddress = new Uri("https://public-api"),
        });
    }

    #region InvokeAsync(GetUserAccessToServiceRequest)

    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserAccessToServiceRequest,
            GetUserAccessToServiceApiRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_GetsExpectedUrl()
    {
        var autoMocker = new AutoMocker();
        UseFakeOptions(autoMocker);

        UseHttpClientWithFakeResponse(autoMocker, /*lang=json,strict*/ """
            { "roles": [] }
        """);

        var requester = autoMocker.CreateInstance<GetUserAccessToServiceApiRequester>();

        await requester.InvokeAsync(new GetUserAccessToServiceRequest {
            UserId = new Guid("cc94a206-6f24-4ac1-ae79-d88a38a9d9be"),
            OrganisationId = new Guid("7e4de903-67f8-4f36-8bd6-a02225c559f4"),
        });

        autoMocker.GetMock<HttpMessageHandler>().Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Get &&
                request.RequestUri == new Uri("https://public-api/services/<client_id>/organisations/7e4de903-67f8-4f36-8bd6-a02225c559f4/users/cc94a206-6f24-4ac1-ae79-d88a38a9d9be")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [TestMethod]
    public Task InvokeAsync_Throws_WhenHasFailingStatusCode()
    {
        var autoMocker = new AutoMocker();
        UseFakeOptions(autoMocker);

        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(""),
            });

        UseHttpClient(autoMocker, new HttpClient(mockHttp.Object) {
            BaseAddress = new Uri("https://public-api"),
        });

        var requester = autoMocker.CreateInstance<GetUserAccessToServiceApiRequester>();

        return Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => requester.InvokeAsync(new GetUserAccessToServiceRequest {
                UserId = new Guid("cc94a206-6f24-4ac1-ae79-d88a38a9d9be"),
                OrganisationId = new Guid("7e4de903-67f8-4f36-8bd6-a02225c559f4"),
            }));
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();
        UseFakeOptions(autoMocker);

        UseHttpClientWithFakeResponse(autoMocker, /*lang=json,strict*/ """
            {
                "roles": [
                    {
                        "id": "c9fb0d97-03f4-4ce3-b050-ac4ab99aabe8",
                        "name": "example-role",
                        "code": "EX123",
                        "numericId": "123",
                        "status": {
                            "id": 1
                        }
                    },
                    {
                        "id": "0c2cb24c-7ced-427d-ac33-c9ac60346312",
                        "name": "another-role",
                        "code": "EX456",
                        "numericId": "456",
                        "status": {
                            "id": 1
                        }
                    }
                ],
                "identities": []
            }
        """);

        var requester = autoMocker.CreateInstance<GetUserAccessToServiceApiRequester>();

        var response = await requester.InvokeAsync(new GetUserAccessToServiceRequest {
            UserId = new Guid("cc94a206-6f24-4ac1-ae79-d88a38a9d9be"),
            OrganisationId = new Guid("7e4de903-67f8-4f36-8bd6-a02225c559f4"),
        });

        Assert.IsNotNull(response);

        var roles = response.Roles.ToArray();
        Assert.AreEqual(2, roles.Length);

        Assert.AreEqual(new Guid("c9fb0d97-03f4-4ce3-b050-ac4ab99aabe8"), roles[0].Id);
        Assert.AreEqual("example-role", roles[0].Name);
        Assert.AreEqual("EX123", roles[0].Code);
        Assert.AreEqual("123", roles[0].NumericId);
        Assert.AreEqual(1, roles[0].Status.Id);

        Assert.AreEqual(new Guid("0c2cb24c-7ced-427d-ac33-c9ac60346312"), roles[1].Id);
        Assert.AreEqual("another-role", roles[1].Name);
        Assert.AreEqual("EX456", roles[1].Code);
        Assert.AreEqual("456", roles[1].NumericId);
        Assert.AreEqual(1, roles[1].Status.Id);
    }

    [TestMethod]
    public Task InvokeAsync_Throws_WhenResponseIsNull()
    {
        var autoMocker = new AutoMocker();
        UseFakeOptions(autoMocker);

        UseHttpClientWithFakeResponse(autoMocker, "null");

        var requester = autoMocker.CreateInstance<GetUserAccessToServiceApiRequester>();

        return Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => requester.InvokeAsync(new GetUserAccessToServiceRequest {
                UserId = new Guid("cc94a206-6f24-4ac1-ae79-d88a38a9d9be"),
                OrganisationId = new Guid("7e4de903-67f8-4f36-8bd6-a02225c559f4"),
            }));
    }

    #endregion
}
