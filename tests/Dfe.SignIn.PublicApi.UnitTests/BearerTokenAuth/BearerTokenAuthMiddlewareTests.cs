using System.Text;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth;

[TestClass]
public class BearerTokenAuthMiddlewareTests
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private DefaultHttpContext context;
    private Mock<IServiceProvider> mockServiceProvider;
    private Mock<IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse>> mockGetServiceApiSecretByServiceId;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private readonly string validJsonContentType = "application/json; charset=utf-8";
    private readonly Mock<RequestDelegate> mockNext = new();
    private readonly string mockValidJwtSecret = "mock-secret";
    private readonly Guid mockValidJwtIss = Guid.Parse("84b30af0-9e5b-4c9d-b8bf-02f0517adab0");
    private readonly string mockJwtWithNothing = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.TVTXWKD524ZPR4i-GWxZVH0QOgGaivxX7FYG1IjWckg";
    private readonly string mockJwtWithInvalidIss = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI4NGIzMGFmMC05ZTViLTRjOWQtYjhiZi0wMmYwNTE3YWRhIiwiYXVkIjoic2lnbmluLmVkdWNhdGlvbi5nb3YudWsifQ.1JqB0uGAQei399WU0kKoYgBXnx0IyWYj4i7qPGLr-3w";

    private readonly string mockJwtWithValidIss = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI4NGIzMGFmMC05ZTViLTRjOWQtYjhiZi0wMmYwNTE3YWRhYjAiLCJhdWQiOiJzaWduaW4uZWR1Y2F0aW9uLmdvdi51ayJ9.HhV2HVhnIAXX2zwjaBoaV_l3b3-H0lKpIA_N1jHScyU";
    private readonly string mockJwtWithValidIssButInvalidAudience = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI4NGIzMGFmMC05ZTViLTRjOWQtYjhiZi0wMmYwNTE3YWRhYjAiLCJhdWQiOiJzaWduaW4uZWR1Y2F0aW9uLmdvdi51In0.GTGfNl7vd1OWvoFm-8oEPS_uLaE3NlXDzSSM5AjNCCc";

    [TestInitialize]
    public void Setup()
    {
        this.context = new DefaultHttpContext();
        this.context.Response.Body = new MemoryStream();
        this.mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        this.mockGetServiceApiSecretByServiceId = new Mock<IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse>>();
        this.mockServiceProvider = new Mock<IServiceProvider>();
        this.mockServiceProvider.Setup(sp => sp.GetService(typeof(IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse>))).Returns(this.mockGetServiceApiSecretByServiceId.Object);
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

    [TestMethod("Return 401 when no authorization header is present")]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenNoAuthorizationHeaderIsPresent()
    {
        this.context.Request.Headers.Append("NoAuthorization", "");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing Authorization header" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 401 when the Authorization header does not contain Bearer")]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenInvalidBearer()
    {
        this.context.Request.Headers.Append("Authorization", "NoBearer");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Malformed Authorization header. Should be bearer {token}" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 401 when an invalid Bearer token is presented")]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenInvalidBearerToken()
    {
        this.context.Request.Headers.Append("Authorization", "Bearer ");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode, "Middleware should return 401 Unauthorized.");
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Invalid token provided" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 403 when a Bearer token is presented, but missing its ISS claim")]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenNothing()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithNothing}");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing or invalid iss claim" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 403 when a Bearer token is provided but the ISS claim is invalid")]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenBearerTokenInvalidIss()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithInvalidIss}");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Missing or invalid iss claim" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 401 when the dependency injected service isn't available")]
    public async Task UseBearerTokenAuthMiddleware_Returns_401_WhenDiServiceNotFound()
    {
        this.mockServiceProvider.Setup(sp => sp.GetService(typeof(IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse>)))
            .Throws(new InvalidOperationException());

        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Service unavailable" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 403 when the associated service does not contain an ApiSecret")]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenNoSecretIsAssignedToService()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockGetServiceApiSecretByServiceId
            .Setup(b => b.InvokeAsync(It.IsAny<GetServiceApiSecretByServiceIdRequest>()))
            .ReturnsAsync(new GetServiceApiSecretByServiceIdResponse {
                Service = new Core.Models.Applications.ServiceApiSecretModel {
                    ApiSecret = null,
                    Id = Guid.Empty
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 403 when the Jwt secret does not match the services secret")]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenJwtSecretDoesntMatch()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockGetServiceApiSecretByServiceId
            .Setup(b => b.InvokeAsync(It.IsAny<GetServiceApiSecretByServiceIdRequest>()))
            .ReturnsAsync(new GetServiceApiSecretByServiceIdResponse {
                Service = new Core.Models.Applications.ServiceApiSecretModel {
                    ApiSecret = "does-not-match-secret",
                    Id = Guid.Empty
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 403 when the Jwt audience does not match the services audience")]
    public async Task UseBearerTokenAuthMiddleware_Returns_403_WhenAudienceDoesntMatch()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIssButInvalidAudience}");

        this.mockGetServiceApiSecretByServiceId
            .Setup(b => b.InvokeAsync(It.IsAny<GetServiceApiSecretByServiceIdRequest>()))
            .ReturnsAsync(new GetServiceApiSecretByServiceIdResponse {
                Service = new Core.Models.Applications.ServiceApiSecretModel {
                    ApiSecret = "does-not-match-secret",
                    Id = Guid.Empty
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(this.validJsonContentType, this.context.Response.ContentType);
        Assert.AreEqual(StatusCodes.Status403Forbidden, this.context.Response.StatusCode);
        Assert.AreEqual(new ErrorResponse { Success = false, Message = "Your client is not authorized to use this api" }, await this.GetResponseBodyAsync());
    }

    [TestMethod("Return 200 and serve the next request delegate when all validation steps have passed")]
    public async Task UseBearerTokenAuthMiddleware_Returns_200_WhenAllValidationStepsPassed()
    {
        this.context.Request.Headers.Append("Authorization", $"Bearer {this.mockJwtWithValidIss}");

        this.mockGetServiceApiSecretByServiceId
            .Setup(b => b.InvokeAsync(It.IsAny<GetServiceApiSecretByServiceIdRequest>()))
            .ReturnsAsync(new GetServiceApiSecretByServiceIdResponse {
                Service = new Core.Models.Applications.ServiceApiSecretModel {
                    ApiSecret = this.mockValidJwtSecret,
                    Id = this.mockValidJwtIss
                }
            });

        var middleware = new BearerTokenAuthMiddleware(this.mockNext.Object, new BearerTokenOptions());
        await middleware.InvokeAsync(this.context);

        Assert.AreEqual(StatusCodes.Status200OK, this.context.Response.StatusCode);
        this.mockNext.Verify(n => n(this.context), Times.Once, "Next middleware should be called exactly once.");
    }
}