using System.Net;
using System.Text;
using Dfe.SignIn.Gateways.Entra.ChangeName;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;

namespace Dfe.SignIn.Gateways.Entra.UnitTests.Features.ChangeName;

[TestClass]
public sealed class EntraChangeNameServiceTests
{
    private Mock<IApplicationGraphClientProvider> graphClientProviderMock = null!;
    private Mock<ILogger<EntraChangeNameService>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        this.graphClientProviderMock = new Mock<IApplicationGraphClientProvider>();
        this.loggerMock = new Mock<ILogger<EntraChangeNameService>>();
    }

    [TestMethod]
    public async Task ChangeNameAsync_WhenExternalUserIdIsEmpty_ReturnsInvalidUserIdFailure()
    {
        // Arrange
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(Guid.Empty, "Jane", "Smith");

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraNameErrors.InvalidUserId, result.Error);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ChangeNameAsync_WhenFirstNameIsNullOrWhiteSpace_ReturnsInvalidFirstNameFailure(string? firstName)
    {
        // Arrange
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(Guid.NewGuid(), firstName!, "Smith");

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraNameErrors.InvalidFirstName, result.Error);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ChangeNameAsync_WhenLastNameIsNullOrWhiteSpace_ReturnsInvalidLastNameFailure(string? lastName)
    {
        // Arrange
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(Guid.NewGuid(), "Jane", lastName!);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraNameErrors.InvalidLastName, result.Error);
    }

    [TestMethod]
    public async Task ChangeNameAsync_WhenPatchSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestsRecorded = new List<HttpRequestMessage>();

        var client = CreateGraphClient(req => {
            requestsRecorded.Add(req);

            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(userId, "Jane", "Smith");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, requestsRecorded.Count);
        Assert.AreEqual(HttpMethod.Patch, requestsRecorded[0].Method);
    }

    [TestMethod]
    public async Task ChangeNameAsync_WhenUserNotFound_ReturnsUserNotFoundFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var client = CreateGraphClient(req => {
            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                var errorContent = /*lang=json,strict*/ """
                {
                    "error": {
                        "code": "Request_ResourceNotFound",
                        "message": "Resource does not exist."
                    }
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(userId, "Jane", "Smith");

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraNameErrors.UserNotFoundCode, result.Error.Code);
        Assert.Contains(userId.ToString(), result.Error.Description);
    }

    [TestMethod]
    public async Task ChangeNameAsync_WhenGeneralODataErrorOccurs_ReturnsUserUpdateFailed()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var client = CreateGraphClient(req => {
            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                var errorContent = /*lang=json,strict*/ """
                {
                    "error": {
                        "code": "GeneralException",
                        "message": "Access was denied."
                    }
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.Forbidden) {
                    Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeNameService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeNameAsync(userId, "Jane", "Smith");

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraNameErrors.UserUpdateFailedCode, result.Error.Code);
        Assert.Contains("Access was denied", result.Error.Description);
    }

    private static GraphServiceClient CreateGraphClient(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
    {
        var handler = new TestHttpMessageHandler(handlerFunc);
        var httpClient = new HttpClient(handler);
        var requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        return new GraphServiceClient(requestAdapter);
    }

    private sealed class TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
        }
    }
}
