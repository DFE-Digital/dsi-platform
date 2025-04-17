using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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

        autoMocker.GetMock<IOptionsMonitor<JsonSerializerOptions>>()
            .Setup(mock => mock.Get(It.Is<string>(key => key == JsonHelperExtensions.StandardOptionsKey)))
            .Returns(JsonHelperExtensions.CreateStandardOptionsTestHelper());
    }

    private static Mock<IHttpContext> SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        mockContext.Setup(mock => mock.Request)
            .Returns(autoMocker.Get<IHttpRequest>());

        mockContext.Setup(mock => mock.Response)
            .Returns(autoMocker.Get<IHttpResponse>());

        return mockContext;

    }

    private static ClaimsPrincipal SetupMockNonAuthenticatedUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakeIdentity = new ClaimsIdentity((IEnumerable<Claim>?)[]);
        var fakeUser = new ClaimsPrincipal(fakeIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    private static readonly Guid FakeUserId = new("8d2799b0-eabe-4a5c-a01f-d52bc1cce63e");
    private static readonly string FakeSessionId = "460ebb6a-643e-4f85-bfe0-29351f11bd62";

    private static ClaimsPrincipal SetupMockAuthenticatedUser(
        AutoMocker autoMocker,
        ClaimsIdentity? dsiIdentity = null)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakePrimaryIdentity = new ClaimsIdentity([
            new(DsiClaimTypes.SessionId, FakeSessionId),
            new(DsiClaimTypes.UserId, FakeUserId.ToString()),
        ], "PrimaryAuthenticationType");

        dsiIdentity ??= new ClaimsIdentity(PublicApiConstants.AuthenticationType);

        var fakeUser = new ClaimsPrincipal([fakePrimaryIdentity, dsiIdentity]);
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
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        mockNext.Verify(next => next(), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware_WhenSignOutPathIsRequested()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/sign-out");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        mockNext.Verify(next => next(), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_InterceptsAndHandlesCallback()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("POST");
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");
        mockRequest.Setup(mock => mock.ReadFormAsync()).ReturnsAsync(
            new Dictionary<string, StringValues> {
                { "payloadType", PayloadTypeConstants.Selection },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "1b1df816-923a-4133-9e91-725a52075645" },
            }
        );

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        autoMocker.GetMock<ISelectOrganisationCallbackProcessor>()
            .Setup(mock => mock.ProcessCallbackAsync(
                It.Is<Guid>(currentUserId => currentUserId == FakeUserId),
                It.Is<SelectOrganisationCallbackViewModel>(viewModel =>
                    viewModel.PayloadType == PayloadTypeConstants.Selection &&
                    viewModel.Payload == "{data}" &&
                    viewModel.Sig == "{sig}" &&
                    viewModel.Kid == "1b1df816-923a-4133-9e91-725a52075645"
                ),
                It.Is<bool>(throwOnError => throwOnError),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new SelectOrganisationCallbackSelection {
                Type = PayloadTypeConstants.Selection,
                UserId = FakeUserId,
                DetailLevel = OrganisationDetailLevel.Id,
                Selection = new SelectedOrganisation {
                    Id = new Guid("c72bdba2-6793-4118-aeba-cd3def045245"),
                },
            });

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IOrganisationClaimManager>(
            x => x.UpdateOrganisationClaimAsync(
                It.IsAny<IHttpContext>(),
                It.Is<string>(organisationJson => organisationJson.Contains("c72bdba2-6793-4118-aeba-cd3def045245")),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        autoMocker.Verify<IHttpResponse>(
            x => x.Redirect(
                It.Is<string>(location => location == "/completed/path")
            ),
            Times.Once
        );

        mockNext.Verify(next => next(), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_RedirectsToSignOutPath_WhenHandlingCancelCallback_AndOrganisationHasNotBeenSelectedYet()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("POST");
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");
        mockRequest.Setup(mock => mock.ReadFormAsync()).ReturnsAsync(
            new Dictionary<string, StringValues> {
                { "payloadType", PayloadTypeConstants.Cancel },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "1b1df816-923a-4133-9e91-725a52075645" },
            }
        );

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        autoMocker.GetMock<ISelectOrganisationCallbackProcessor>()
            .Setup(mock => mock.ProcessCallbackAsync(
                It.Is<Guid>(currentUserId => currentUserId == FakeUserId),
                It.Is<SelectOrganisationCallbackViewModel>(viewModel =>
                    viewModel.PayloadType == PayloadTypeConstants.Cancel &&
                    viewModel.Payload == "{data}" &&
                    viewModel.Sig == "{sig}" &&
                    viewModel.Kid == "1b1df816-923a-4133-9e91-725a52075645"
                ),
                It.Is<bool>(throwOnError => throwOnError),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new SelectOrganisationCallbackCancel {
                Type = PayloadTypeConstants.Cancel,
                UserId = FakeUserId,
            });

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IHttpResponse>(
            x => x.Redirect(
                It.Is<string>(location => location == "/sign-out")
            ),
            Times.Once
        );

        mockNext.Verify(next => next(), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_DoesNotRedirectToSignOutPath_WhenHandlingCancelCallback_AndOrganisationHasBeenSelected()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);

        var fakeDsiIdentity = new ClaimsIdentity(
            [
                new(DsiClaimTypes.SessionId, FakeSessionId),
                new(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json),
            ],
            PublicApiConstants.AuthenticationType
        );
        SetupMockAuthenticatedUser(autoMocker, fakeDsiIdentity);

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("POST");
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");
        mockRequest.Setup(mock => mock.ReadFormAsync()).ReturnsAsync(
            new Dictionary<string, StringValues> {
                { "payloadType", PayloadTypeConstants.Cancel },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "1b1df816-923a-4133-9e91-725a52075645" },
            }
        );

        var mockContext = SetupMockHttpContext(autoMocker);

        autoMocker.GetMock<ISelectOrganisationCallbackProcessor>()
            .Setup(mock => mock.ProcessCallbackAsync(
                It.Is<Guid>(currentUserId => currentUserId == FakeUserId),
                It.Is<SelectOrganisationCallbackViewModel>(viewModel =>
                    viewModel.PayloadType == PayloadTypeConstants.Cancel &&
                    viewModel.Payload == "{data}" &&
                    viewModel.Sig == "{sig}" &&
                    viewModel.Kid == "1b1df816-923a-4133-9e91-725a52075645"
                ),
                It.Is<bool>(throwOnError => throwOnError),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new SelectOrganisationCallbackCancel {
                Type = PayloadTypeConstants.Cancel,
                UserId = FakeUserId,
            });

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        await middleware.InvokeAsync(mockContext.Object, () => Task.CompletedTask);

        autoMocker.Verify<IHttpResponse>(
            x => x.Redirect(
                It.Is<string>(location => location == "/sign-out")
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task InvokeAsync_RedirectsToSignOutPath_WhenHandlingSignOutCallback()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("POST");
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");
        mockRequest.Setup(mock => mock.ReadFormAsync()).ReturnsAsync(
            new Dictionary<string, StringValues> {
                { "payloadType", PayloadTypeConstants.SignOut },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "1b1df816-923a-4133-9e91-725a52075645" },
            }
        );

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        autoMocker.GetMock<ISelectOrganisationCallbackProcessor>()
            .Setup(mock => mock.ProcessCallbackAsync(
                It.Is<Guid>(currentUserId => currentUserId == FakeUserId),
                It.Is<SelectOrganisationCallbackViewModel>(viewModel =>
                    viewModel.PayloadType == PayloadTypeConstants.SignOut &&
                    viewModel.Payload == "{data}" &&
                    viewModel.Sig == "{sig}" &&
                    viewModel.Kid == "1b1df816-923a-4133-9e91-725a52075645"
                ),
                It.Is<bool>(throwOnError => throwOnError),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new SelectOrganisationCallbackSignOut {
                Type = PayloadTypeConstants.SignOut,
                UserId = FakeUserId,
            });

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IHttpResponse>(
            x => x.Redirect(
                It.Is<string>(location => location == "/sign-out")
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

        var fakeDsiIdentity = new ClaimsIdentity(
            [
                new(DsiClaimTypes.SessionId, FakeSessionId),
                new(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json),
            ],
            PublicApiConstants.AuthenticationType
        );
        SetupMockAuthenticatedUser(autoMocker, fakeDsiIdentity);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.IsAny<IHttpContext>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        mockNext.Verify(next => next(), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_InitiatesSelectOrganisation_WhenUserHasRequestedSelectOrganisation_AndIsEnabled()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });

        var fakeDsiIdentity = new ClaimsIdentity(
            [
                new(DsiClaimTypes.SessionId, FakeSessionId),
                new(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json),
            ],
            PublicApiConstants.AuthenticationType
        );
        SetupMockAuthenticatedUser(autoMocker, fakeDsiIdentity);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        mockNext.Verify(next => next(), Times.Never);
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
        var mockNext = new Mock<Func<Task>>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        mockNext.Verify(next => next(), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_InitiatesSelectOrganisation_WhenExistingSelectionWasForDifferentUser()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });

        var fakeDsiIdentity = new ClaimsIdentity(
            [
                new(DsiClaimTypes.SessionId, "050cb409-55be-430c-9160-f40e875d5b40"),
                new(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json),
            ],
            PublicApiConstants.AuthenticationType
        );
        SetupMockAuthenticatedUser(autoMocker, fakeDsiIdentity);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();
        var mockNext = new Mock<Func<Task>>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        mockNext.Verify(next => next(), Times.Never);
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
        var fakeDsiIdentity = new ClaimsIdentity(
            [
                new(DsiClaimTypes.SessionId, FakeSessionId),
                new(DsiClaimTypes.Organisation, organisationClaim, JsonClaimValueTypes.Json),
            ],
            PublicApiConstants.AuthenticationType
        );
        SetupMockAuthenticatedUser(autoMocker, fakeDsiIdentity);

        var middleware = autoMocker.CreateInstance<AuthenticationOrganisationSelectorMiddleware>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<IAuthenticationOrganisationSelector>(
            x => x.InitiateSelectionAsync(
                It.IsAny<IHttpContext>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        mockNext.Verify(next => next(), Times.Once);
    }

    #endregion
}
