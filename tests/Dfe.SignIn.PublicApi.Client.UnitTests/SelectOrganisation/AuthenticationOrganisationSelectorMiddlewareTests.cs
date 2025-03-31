using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class AuthenticationOrganisationSelectorMiddlewareTests
{
    private static void SetupOptions(
        AutoMocker autoMocker,
        AuthenticationOrganisationSelectorOptions? options = null)
    {
        autoMocker.GetMock<IOptions<AuthenticationOrganisationSelectorOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new AuthenticationOrganisationSelectorOptions {
                CompletedPath = "/completed/path",
            });

        autoMocker.Use(JsonHelperExtensions.CreateStandardOptions());
    }

    private static Mock<HttpContext> SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        mockContext.Setup(mock => mock.Request)
            .Returns(autoMocker.Get<HttpRequest>());

        mockContext.Setup(mock => mock.Response)
            .Returns(autoMocker.Get<HttpResponse>());

        return mockContext;

    }

    private static ClaimsPrincipal SetupMockNonAuthenticatedUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        var fakeIdentity = new ClaimsIdentity((IEnumerable<Claim>?)[]);
        var fakeUser = new ClaimsPrincipal(fakeIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    private static readonly Guid FakeUserId = new("8d2799b0-eabe-4a5c-a01f-d52bc1cce63e");

    private static ClaimsPrincipal SetupMockAuthenticatedUser(
        AutoMocker autoMocker,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        var fakeIdentity = new ClaimsIdentity((IEnumerable<Claim>?)[
            new Claim(DsiClaimTypes.UserId, FakeUserId.ToString()),
            ..additionalClaims ?? []
        ], authenticationType: "TestAuth");

        var fakeUser = new ClaimsPrincipal(fakeIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    #region InvokeAsync(HttpContext)

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockNonAuthenticatedUser(autoMocker);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<RequestDelegate>(
            x => x.Invoke(
                It.Is<HttpContext>(context => context == mockContext.Object)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_InterceptsAndHandlesCallback()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns(HttpMethods.Post);
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");
        mockRequest.Setup(mock => mock.Form).Returns(new FormCollection(new() {
            { "payloadType", PayloadTypeConstants.Id },
            { "payload", "{data}" },
            { "sig", "{sig}" },
            { "kid", "1b1df816-923a-4133-9e91-725a52075645" },
        }));

        var mockContext = SetupMockHttpContext(autoMocker);

        autoMocker.GetMock<ISelectOrganisationCallbackProcessor>()
            .Setup(mock => mock.ProcessCallbackAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel =>
                    viewModel.PayloadType == PayloadTypeConstants.Id &&
                    viewModel.Payload == "{data}" &&
                    viewModel.Sig == "{sig}" &&
                    viewModel.Kid == "1b1df816-923a-4133-9e91-725a52075645"
                ),
                It.Is<bool>(throwOnError => throwOnError),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new SelectOrganisationCallbackId {
                Type = PayloadTypeConstants.Id,
                Id = new Guid("c72bdba2-6793-4118-aeba-cd3def045245"),
            });

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<IOrganisationClaimManager>(
            x => x.UpdateOrganisationClaimAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(organisationJson => organisationJson.Contains("c72bdba2-6793-4118-aeba-cd3def045245")),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        autoMocker.Verify<HttpResponse>(
            x => x.Redirect(
                It.Is<string>(location => location == "/completed/path")
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware_WhenUserHasRequestedSelectOrganisation_AndIsNotEnabled()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = false,
        });
        SetupMockAuthenticatedUser(autoMocker, [
            new Claim(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json)
        ]);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns(HttpMethods.Get);
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        autoMocker.Verify<RequestDelegate>(
            x => x.Invoke(
                It.Is<HttpContext>(context => context == mockContext.Object)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_InitiatesSelectOrganisation_WhenUserHasRequestedSelectOrganisation_AndIsEnabled()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });
        SetupMockAuthenticatedUser(autoMocker, [
            new Claim(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json)
        ]);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns(HttpMethods.Get);
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.Is<HttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_InitiatesSelectOrganisation_WhenUserHasNotSelectedAnOrganisation()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });
        SetupMockAuthenticatedUser(autoMocker);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns(HttpMethods.Get);
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.Is<HttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_DoesNotInitiateSelectOrganisation_WhenUserHasAlreadySelectedAnOrganisation()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });

        string organisationClaim = /*lang=json,strict*/ """
            { "id": "1bce763f-cb38-49a8-813c-a786a753f0eb" }
        """;
        SetupMockAuthenticatedUser(autoMocker, [
            new Claim(DsiClaimTypes.Organisation, organisationClaim, JsonClaimValueTypes.Json)
        ]);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns(HttpMethods.Get);
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        autoMocker.Verify<RequestDelegate>(
            x => x.Invoke(
                It.Is<HttpContext>(context => context == mockContext.Object)
            ),
            Times.Once
        );
    }

    #endregion
}
