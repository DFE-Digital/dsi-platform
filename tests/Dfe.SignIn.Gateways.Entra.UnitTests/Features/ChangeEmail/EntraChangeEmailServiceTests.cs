using System.Net;
using System.Text;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;

namespace Dfe.SignIn.Gateways.Entra.UnitTests.Features.ChangeEmail;

[TestClass]
public sealed class EntraChangeEmailServiceTests
{
    private Mock<IApplicationGraphClientProvider> graphClientProviderMock = null!;
    private Mock<ILogger<EntraChangeEmailService>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        this.graphClientProviderMock = new Mock<IApplicationGraphClientProvider>();
        this.loggerMock = new Mock<ILogger<EntraChangeEmailService>>();
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenNewEmailIsNullOrWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            sut.ChangeEmailAsync(Guid.NewGuid(), "   "));
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenExternalUserIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            sut.ChangeEmailAsync(Guid.Empty, "updated.user@example.com"));
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenBothPrimaryAndExistingMfaSucceed_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string newEmail = "updated.user@example.com";
        var requestsRecorded = new List<HttpRequestMessage>();

        var client = CreateGraphClient(req => {
            requestsRecorded.Add(req);

            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}/authentication/emailMethods")) {
                var content = /*lang=json,strict*/ """
                {
                    "value": [
                        {
                            "id": "existing-mfa-method-id",
                            "emailAddress": "old.user@example.com"
                        }
                    ]
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
            }

            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.Contains("existing-mfa-method-id")) {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeEmailAsync(userId, newEmail);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, requestsRecorded.Count);
        Assert.AreEqual(HttpMethod.Patch, requestsRecorded[0].Method);
        Assert.AreEqual(HttpMethod.Get, requestsRecorded[1].Method);
        Assert.AreEqual(HttpMethod.Patch, requestsRecorded[2].Method);
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenNoExistingMfaMethod_PostsNewMfaMethod_AndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string newEmail = "new.user@example.com";
        var requestsRecorded = new List<HttpRequestMessage>();

        var client = CreateGraphClient(req => {
            requestsRecorded.Add(req);

            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}/authentication/emailMethods")) {
                var content = /*lang=json,strict*/ """
                {
                    "value": []
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
            }

            if (req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}/authentication/emailMethods")) {
                var responseContent = /*lang=json,strict*/ """
                {
                    "id": "created-mfa-id",
                    "emailAddress": "new.user@example.com"
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.Created) {
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeEmailAsync(userId, newEmail);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, requestsRecorded.Count);
        Assert.AreEqual(HttpMethod.Patch, requestsRecorded[0].Method);
        Assert.AreEqual(HttpMethod.Get, requestsRecorded[1].Method);
        Assert.AreEqual(HttpMethod.Post, requestsRecorded[2].Method);
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenPrimaryUserUpdateFails_ReturnsUserUpdateFailed_AndDoesNotAttemptMfa()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string newEmail = "new.user@example.com";
        var requestsRecorded = new List<HttpRequestMessage>();

        var client = CreateGraphClient(req => {
            requestsRecorded.Add(req);

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
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeEmailAsync(userId, newEmail);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraEmailErrors.UserUpdateFailed("Resource does not exist."), result.Error);
        Assert.Contains("Resource does not exist", result.Error.Description);
        Assert.AreEqual(1, requestsRecorded.Count);
    }

    [TestMethod]
    public async Task ChangeEmailAsync_WhenPrimarySucceeds_AndMfaFails_ReturnsMfaAuthenticationMethodFailed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string newEmail = "new.user@example.com";
        var requestsRecorded = new List<HttpRequestMessage>();

        var client = CreateGraphClient(req => {
            requestsRecorded.Add(req);

            if (req.Method == HttpMethod.Patch && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}")) {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith($"/users/{userId}/authentication/emailMethods")) {
                var errorContent = /*lang=json,strict*/ """
                {
                    "error": {
                        "code": "GeneralException",
                        "message": "Failed to retrieve MFA methods."
                    }
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) {
                    Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        this.graphClientProviderMock.Setup(x => x.GetClient()).Returns(client);
        var sut = new EntraChangeEmailService(this.graphClientProviderMock.Object, this.loggerMock.Object);

        // Act
        var result = await sut.ChangeEmailAsync(userId, newEmail);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(EntraEmailErrors.MfaAuthenticationMethodFailed("Failed to retrieve MFA methods."), result.Error);
        Assert.Contains("Failed to retrieve MFA methods", result.Error.Description);
        Assert.AreEqual(2, requestsRecorded.Count);
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
