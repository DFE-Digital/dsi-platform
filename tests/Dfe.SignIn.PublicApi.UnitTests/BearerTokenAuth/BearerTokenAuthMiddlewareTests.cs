using System.Text;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.AspNetCore.Http;

using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth;

[TestClass]
public class BearerTokenAuthMiddlewareTests
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private DefaultHttpContext context;
    private ScopedSession.ScopedSessionProvider mockScopedSessionProvider;
    private Mock<IServiceProvider> mockServiceProvider;
    private Mock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>> mockApplicationByClientId;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private readonly string validJsonContentType = "application/json; charset=utf-8";
    private readonly Mock<RequestDelegate> mockNext = new();
    private readonly string mockValidJwtSecret = "mock-secret";
    private readonly string mockValidJwtIss = "my-application-name";
    private readonly string mockJwtWithNothing = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.TVTXWKD524ZPR4i-GWxZVH0QOgGaivxX7FYG1IjWckg";
    private readonly string mockJwtWithInvalidIss = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJpbnZhbGlkLWlzcyIsImF1ZCI6InNpZ25pbi5lZHVjYXRpb24uZ292LnVrIn0.gq2YEHiJR1UNNyRWaFTlwrsej89mss2kAGkViKVQKQM";
    private readonly string mockJwtWithNoIss = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJzaWduaW4uZWR1Y2F0aW9uLmdvdi51In0.PDc1pYWUQK52H59Rs3wGBceJqam4IkhuOfDryFZUe8s";
    private readonly string mockJwtWithValidIss = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJteS1hcHBsaWNhdGlvbi1uYW1lIiwiYXVkIjoic2lnbmluLmVkdWNhdGlvbi5nb3YudWsifQ.nEn6xJz26M9gFxBd0_iVGCY_QYpe0BKKR5RItnivDFo";
    private readonly string mockJwtWithValidIssButInvalidAudience = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJteS1hcHBsaWNhdGlvbi1uYW1lIiwiYXVkIjoic2lnbmluLmVkdWNhdGlvbi5nb3YudSJ9.kBFYCJsNTG9rlDacZ9OUG2wN53zi450uRhU7OYJUR5w";

    private readonly string mockJwtWithExpExpired = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJteS1hcHBsaWNhdGlvbi1uYW1lIiwiYXVkIjoic2lnbmluLmVkdWNhdGlvbi5nb3YudWsiLCJleHAiOjk1MzY0Nzg0Nn0.k21UWtkrs7w6uhphYawpWJfvGu_zQWbfP54AgI7k4aE";
    private readonly string mockJwtWithValidExp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJteS1hcHBsaWNhdGlvbi1uYW1lIiwiYXVkIjoic2lnbmluLmVkdWNhdGlvbi5nb3YudWsiLCJleHAiOjMyNTEwNTU3NjI4fQ.EdOSJbFSkCPnmvqOE5USDaCt2S2uGO1HI-5BPyjvw80";

    [TestInitialize]
    public void Setup()
    {
        this.context = new DefaultHttpContext();
        this.context.Response.Body = new MemoryStream();
        this.mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        this.mockScopedSessionProvider = new() { Application = default! };

        this.mockApplicationByClientId = new Mock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>();
        this.mockServiceProvider = new Mock<IServiceProvider>();
        this.mockServiceProvider.Setup(sp => sp.GetService(typeof(IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>))).Returns(this.mockApplicationByClientId.Object);
        this.mockServiceProvider.Setup(sp => sp.GetService(typeof(IScopedSessionReader))).Returns(this.mockScopedSessionProvider);
        this.mockServiceProvider.Setup(sp => sp.GetService(typeof(IScopedSessionWriter))).Returns(this.mockScopedSessionProvider);

        this.context.RequestServices = this.mockServiceProvider.Object;
    }

    private record ErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }

    private async Task<ErrorResponse?> GetResponseBodyAsync()
    {
        this.context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(this.context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        return System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(responseText);
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenNoAuthorizationHeaderIsPresent()
    {
        this.context.Request.Headers.Append("NoAuthorization", "");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing Authorization header" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenInvalidBearer()
    {
        this.context.Request.Headers.Append("Authorization", "NoBearer");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Malformed Authorization header. Should be bearer {token}" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenInvalidBearerToken()
    {
        this.context.Request.Headers.Append("Authorization", "Bearer ");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Invalid token provided" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenNothing()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithNothing}");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing or invalid iss claim" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenHasNoIss()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithNoIss}");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing or invalid iss claim" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenInvalidIss()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithInvalidIss}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = this.mockValidJwtSecret,
                    ClientId = "mock-invalid-client-id",
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenTheAssociatedServiceReturnsNullApplication()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = null
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Unknown issuer" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenNoSecretIsAssignedToService()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    Id = Guid.Empty,
                    ApiSecret = null,
                    ClientId = "mock-client-id",
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenJwtSecretDoesntMatch()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = "does-not-match-secret",
                    ClientId = string.Empty,
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenAudienceDoesntMatch()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIssButInvalidAudience}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = "does-not-match-secret",
                    ClientId = string.Empty,
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenExpired()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithExpExpired}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = this.mockValidJwtSecret,
                    ClientId = this.mockValidJwtIss,
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "jwt expired" }, await this.GetResponseBodyAsync());
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_200_WhenAllValidationStepsPassed()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = this.mockValidJwtSecret,
                    ClientId = this.mockValidJwtIss,
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(StatusCodes.Status200OK, this.context.Response.StatusCode);
        this.mockNext.Verify(n => n(this.context), Times.Once, "Next middleware should be called exactly once.");
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_Returns_200_WhenAllValidationStepsPassedWithValidExp()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidExp}");

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = new Core.Models.Applications.ApplicationModel {
                    ApiSecret = this.mockValidJwtSecret,
                    ClientId = this.mockValidJwtIss,
                    Id = Guid.Empty,
                    Description = "mock-description",
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                    Name = "mock-name",
                    ServiceHomeUrl = new Uri("https://localhost")
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(StatusCodes.Status200OK, this.context.Response.StatusCode);
        this.mockNext.Verify(n => n(this.context), Times.Once, "Next middleware should be called exactly once.");
    }

    [TestMethod]
    public async Task UseBearerTokenAuthMiddleware_ShouldSetScopedSessionVariables()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        var application = new Core.Models.Applications.ApplicationModel {
            ApiSecret = this.mockValidJwtSecret,
            ClientId = this.mockValidJwtIss,
            Id = Guid.Empty,
            Description = "mock-description",
            IsExternalService = false,
            IsHiddenService = false,
            IsIdOnlyService = false,
            Name = "mock-name",
            ServiceHomeUrl = new Uri("https://localhost")
        };

        this.mockApplicationByClientId
            .Setup(b => b.InvokeAsync(It.IsAny<GetApplicationByClientIdRequest>()))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = application
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreSame(this.mockScopedSessionProvider.Application, application);
    }
}
