using System.Security.Claims;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.Contracts.Organisations;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class StandardSelectOrganisationMiddlewareTests
{
    private static readonly OrganisationDetails FakeOrganisation = new() {
        Id = new Guid("1bce763f-cb38-49a8-813c-a786a753f0eb"),
        Name = "Example Organisation 1",
    };

    private static void SetupOptions(
        AutoMocker autoMocker,
        StandardSelectOrganisationUserFlowOptions? options = null)
    {
        autoMocker.GetMock<IOptions<StandardSelectOrganisationUserFlowOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new StandardSelectOrganisationUserFlowOptions {
                CompletedPath = "/completed/path",
            });

        autoMocker.UseStandardJsonSerializerOptions();
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

    private static ClaimsPrincipal SetupMockAuthenticatedUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakePrimaryIdentity = new ClaimsIdentity([
            new(DsiClaimTypes.SessionId, FakeSessionId),
            new(DsiClaimTypes.UserId, FakeUserId.ToString()),
        ], "PrimaryAuthenticationType");

        var fakeUser = new ClaimsPrincipal(fakePrimaryIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    private static void SetupMockActiveOrganisation(AutoMocker autoMocker, OrganisationDetails? organisation)
    {
        autoMocker.GetMock<IActiveOrganisationProvider>()
            .Setup(mock => mock.GetActiveOrganisationStateAsync(
                It.IsAny<IHttpContext>()
            ))
            .ReturnsAsync(new ActiveOrganisationState { Organisation = organisation });
    }

    #region InvokeAsync(HttpContext)

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker);
        SetupMockNonAuthenticatedUser(autoMocker);

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

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

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

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
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/callback/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<ISelectOrganisationUserFlow>(mock =>
            mock.ProcessCallbackAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        mockNext.Verify(next => next(), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware_WhenUserHasRequestedSelectOrganisation_AndIsNotEnabled()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = false,
        });
        SetupMockAuthenticatedUser(autoMocker);
        SetupMockActiveOrganisation(autoMocker, null);

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        var mockFlow = autoMocker.GetMock<ISelectOrganisationUserFlow>();
        mockFlow.VerifyNoOtherCalls();

        mockNext.Verify(next => next(), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_InitiatesSelectOrganisation_WhenUserHasRequestedSelectOrganisation_AndIsEnabled()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });
        SetupMockAuthenticatedUser(autoMocker);
        SetupMockActiveOrganisation(autoMocker, null);

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<ISelectOrganisationUserFlow>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        mockNext.Verify(next => next(), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_AllowsCancel_WhenHasActiveOrganisation()
    {
        var autoMocker = new AutoMocker();
        SetupOptions(autoMocker, new() {
            EnableSelectOrganisationRequests = true,
        });
        SetupMockAuthenticatedUser(autoMocker);
        SetupMockActiveOrganisation(autoMocker, FakeOrganisation);

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/select-organisation");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<ISelectOrganisationUserFlow>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.Is<bool>(allowCancel => allowCancel),
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

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();
        var mockNext = new Mock<Func<Task>>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        autoMocker.Verify<ISelectOrganisationUserFlow>(
            x => x.InitiateSelectionAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.Is<bool>(allowCancel => !allowCancel),
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
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockActiveOrganisation(autoMocker, FakeOrganisation);

        autoMocker.GetMock<IActiveOrganisationProvider>()
            .Setup(mock => mock.GetActiveOrganisationStateAsync(
                It.IsAny<IHttpContext>()
            ))
            .ReturnsAsync(new ActiveOrganisationState {
                Organisation = FakeOrganisation,
            });

        var middleware = autoMocker.CreateInstance<StandardSelectOrganisationMiddleware>();

        // Mock a request to some page within the application.
        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("GET");
        mockRequest.Setup(mock => mock.Path).Returns("/profile");

        var mockContext = SetupMockHttpContext(autoMocker);
        var mockNext = new Mock<Func<Task>>();

        await middleware.InvokeAsync(mockContext.Object, mockNext.Object);

        var mockFlow = autoMocker.GetMock<ISelectOrganisationUserFlow>();
        mockFlow.VerifyNoOtherCalls();

        mockNext.Verify(next => next(), Times.Once);
    }

    #endregion
}
