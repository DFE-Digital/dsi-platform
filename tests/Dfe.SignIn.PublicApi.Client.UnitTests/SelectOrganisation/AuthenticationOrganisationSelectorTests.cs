using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class AuthenticationOrganisationSelectorTests
{
    private static void SetupMockOptions(AutoMocker autoMocker, AuthenticationOrganisationSelectorOptions? options = null)
    {
        autoMocker.GetMock<IOptions<AuthenticationOrganisationSelectorOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new AuthenticationOrganisationSelectorOptions {
                CompletedPath = "/completed/path",
            });
    }

    private static Mock<IHttpContext> SetupMockContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var mockRequest = autoMocker.GetMock<IHttpRequest>();
        mockRequest.Setup(mock => mock.Scheme).Returns("http");
        mockRequest.Setup(mock => mock.Host).Returns("localhost");
        mockRequest.Setup(mock => mock.PathBase).Returns("/app");
        mockContext.Setup(mock => mock.Request)
            .Returns(mockRequest.Object);

        var mockResponse = autoMocker.GetMock<IHttpResponse>();
        mockContext.Setup(mock => mock.Response)
            .Returns(mockResponse.Object);

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

    private static ClaimsPrincipal SetupMockAuthenticatedUser(
        AutoMocker autoMocker,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakeIdentity = new ClaimsIdentity((IEnumerable<Claim>?)[
            new Claim(DsiClaimTypes.UserId, FakeUserId.ToString()),
            ..additionalClaims ?? []
        ], authenticationType: "TestAuth");

        var fakeUser = new ClaimsPrincipal(fakeIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    private static readonly CreateSelectOrganisationSession_PublicApiResponse FakeCreateSelectOrganisationResponse_WithOptions = new() {
        HasOptions = true,
        Url = new Uri("https://select-organisation.localhost"),
    };

    private static readonly CreateSelectOrganisationSession_PublicApiResponse FakeCreateSelectOrganisationResponse_WithNoOptions = new() {
        HasOptions = false,
        Url = new Uri("https://select-organisation.localhost"),
    };

    private static Mock<
        IInteractor<CreateSelectOrganisationSession_PublicApiRequest, CreateSelectOrganisationSession_PublicApiResponse>
    > SetupMockCreateSelectOrganisationSession(AutoMocker autoMocker, CreateSelectOrganisationSession_PublicApiResponse response)
    {
        var mockRequester = autoMocker.GetMock<
            IInteractor<CreateSelectOrganisationSession_PublicApiRequest, CreateSelectOrganisationSession_PublicApiResponse>>();

        mockRequester
            .Setup(mock => mock.InvokeAsync(
                It.IsAny<CreateSelectOrganisationSession_PublicApiRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(response);

        return mockRequester;
    }

    #region InitiateSelectionAsync(HttpContext)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task InitiateSelectionAsync_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(null!);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_Throws_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockNonAuthenticatedUser(autoMocker);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => selector.InitiateSelectionAsync(fakeContext.Object)
        );
        Assert.AreEqual(
            "Cannot initiate organisation selection since user is not authenticated.",
            exception.Message
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_PostRequestToSelectOrganisationEndpoint()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(
            autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        mockCreateSelectOrganisationSession.Verify(x => x.InvokeAsync(
            It.IsAny<CreateSelectOrganisationSession_PublicApiRequest>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_InteractsTo_CreateSelectOrganisationSession()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(
            autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        mockCreateSelectOrganisationSession.Verify(x => x.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                request.UserId == FakeUserId &&
                request.CallbackUrl == new Uri("http://localhost/app/callback/select-organisation")
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_InteractsTo_CreateSelectOrganisationSession_WithPrepareSelectOrganisationRequest()
    {
        var autoMocker = new AutoMocker();

        SetupMockOptions(autoMocker, new() {
            PrepareSelectOrganisationRequest = (request) => request with {
                Prompt = new SelectOrganisationPrompt {
                    Heading = "Custom prompt heading",
                },
            },
        });

        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(
            autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        mockCreateSelectOrganisationSession.Verify(x => x.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                request.UserId == FakeUserId &&
                request.CallbackUrl == new Uri("http://localhost/app/callback/select-organisation") &&
                request.Prompt.Heading == "Custom prompt heading"
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_RedirectsToSelectOrganisationUrl_WhenHasOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(location => location == "https://select-organisation.localhost/")
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_UpdatesOrganisationClaimToBeNull_WhenHasNoOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        autoMocker.Verify<IOrganisationClaimManager>(x =>
            x.UpdateOrganisationClaimAsync(
                It.IsAny<IHttpContext>(),
                It.Is<string>(organisationJson => organisationJson == "null"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_RedirectsToCompletedPath_WhenHasNoOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(location => location == "/completed/path")
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_AllowCancelIsFalse_WhenUserIsSelectingOrganisationForFirstTime()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        mockCreateSelectOrganisationSession.Verify(x => x.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                !request.AllowCancel
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_AllowCancelIsTrue_WhenUserIsSwitchingToAnotherOrganisation()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);

        SetupMockAuthenticatedUser(autoMocker, [
            new Claim(DsiClaimTypes.Organisation, "null", JsonClaimValueTypes.Json)
        ]);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<AuthenticationOrganisationSelector>();

        await selector.InitiateSelectionAsync(fakeContext.Object);

        mockCreateSelectOrganisationSession.Verify(x => x.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                request.AllowCancel
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion
}
