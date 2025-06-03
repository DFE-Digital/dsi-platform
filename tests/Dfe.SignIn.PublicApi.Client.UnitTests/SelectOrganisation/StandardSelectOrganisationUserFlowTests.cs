using System.Security.Claims;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Users;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class StandardSelectOrganisationUserFlowTests
{
    private static void SetupMockOptions(AutoMocker autoMocker, StandardSelectOrganisationUserFlowOptions? options = null)
    {
        autoMocker.GetMock<IOptions<StandardSelectOrganisationUserFlowOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new StandardSelectOrganisationUserFlowOptions {
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
    private static readonly string FakeSessionId = "460ebb6a-643e-4f85-bfe0-29351f11bd62";

    private static readonly CreateSelectOrganisationSession_PublicApiResponse FakeCreateSelectOrganisationResponse_WithOptions = new() {
        RequestId = new Guid("8c1225d8-65a5-4372-8025-61c0c63a323a"),
        HasOptions = true,
        Url = new Uri("https://select-organisation.localhost"),
    };

    private static readonly CreateSelectOrganisationSession_PublicApiResponse FakeCreateSelectOrganisationResponse_WithNoOptions = new() {
        RequestId = new Guid("78f29d5a-ca9d-4605-96ea-b3ef789131d2"),
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

    #region InitiateSelectionAsync(IHttpContext, bool, CancellationToken)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task InitiateSelectionAsync_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var flow = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await flow.InitiateSelectionAsync(null!, false, default);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_Throws_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockNonAuthenticatedUser(autoMocker);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => selector.InitiateSelectionAsync(fakeContext.Object, false, default)
        );
        Assert.AreEqual("User is not authenticated.", exception.Message);
    }

    [DataRow(false)]
    [DataRow(true)]
    [DataTestMethod]
    public async Task InitiateSelectionAsync_InteractsTo_CreateSelectOrganisationSession(bool allowCancel)
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        var mockCreateSelectOrganisationSession = SetupMockCreateSelectOrganisationSession(
            autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, allowCancel, default);

        mockCreateSelectOrganisationSession.Verify(mock => mock.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                request.UserId == FakeUserId &&
                request.CallbackUrl == new Uri("http://localhost/app/callback/select-organisation") &&
                request.AllowCancel == allowCancel
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

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, false, default);

        mockCreateSelectOrganisationSession.Verify(mock => mock.InvokeAsync(
            It.Is<CreateSelectOrganisationSession_PublicApiRequest>(request =>
                request.UserId == FakeUserId &&
                request.CallbackUrl == new Uri("http://localhost/app/callback/select-organisation") &&
                !request.AllowCancel &&
                request.Prompt.Heading == "Custom prompt heading"
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_StartsTrackingSelectOrganisationRequest_WhenHasOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, false, default);

        autoMocker.Verify<ISelectOrganisationRequestTrackingProvider>(mock =>
            mock.SetTrackedRequestAsync(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<Guid?>(requestId => requestId == FakeCreateSelectOrganisationResponse_WithOptions.RequestId)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_RaisesEvent_OnStartSelection_WhenHasOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithOptions);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, false, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnStartSelection(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<Uri>(selectionUri => selectionUri == new Uri("https://select-organisation.localhost"))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_ClearsTrackingSelectOrganisationRequest_WhenHasNoOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, false, default);

        autoMocker.Verify<ISelectOrganisationRequestTrackingProvider>(mock =>
            mock.SetTrackedRequestAsync(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<Guid?>(requestId => requestId == null)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InitiateSelectionAsync_RaisesEvent_OnConfirmSelectionWithNull_WhenHasNoOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        SetupMockCreateSelectOrganisationSession(autoMocker, FakeCreateSelectOrganisationResponse_WithNoOptions);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.InitiateSelectionAsync(fakeContext.Object, false, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnConfirmSelection(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<OrganisationDetails?>(organisation => organisation == null)
            ),
            Times.Once
        );
    }

    #endregion

    #region ProcessCallbackAsync(IHttpContext, CancellationToken)

    private const string FakeRequestId = "ef57cd31-5336-4bf5-be11-59628a698d3b";
    private const string FakeOrganisationId = "ad27819b-2991-40ba-9d5f-8fd9da010aa5";

    private static readonly OrganisationDetails FakeOrganisation = new() {
        Id = new Guid(FakeOrganisationId),
        Name = "Example Organisation 1",
    };

    private static void SetupMockRequestTracker(AutoMocker autoMocker, Guid trackedRequestId)
    {
        autoMocker.GetMock<ISelectOrganisationRequestTrackingProvider>()
            .Setup(mock => mock.IsTrackingRequestAsync(
                It.IsAny<IHttpContext>(),
                It.Is<Guid>(requestId => requestId != trackedRequestId)
            ))
            .ReturnsAsync(false);

        autoMocker.GetMock<ISelectOrganisationRequestTrackingProvider>()
            .Setup(mock => mock.IsTrackingRequestAsync(
                It.IsAny<IHttpContext>(),
                It.Is<Guid>(requestId => requestId == trackedRequestId)
            ))
            .ReturnsAsync(true);
    }

    private static void MockQueryParam(AutoMocker autoMocker, string key, object value)
    {
        autoMocker.GetMock<IHttpRequest>()
            .Setup(mock => mock.GetQuery(It.Is<string>(k => k == key)))
            .Returns(value.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var flow = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await flow.ProcessCallbackAsync(null!, default);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockNonAuthenticatedUser(autoMocker);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => selector.ProcessCallbackAsync(fakeContext.Object, default)
        );
        Assert.AreEqual("User is not authenticated.", exception.Message);
    }

    [TestMethod]
    [ExpectedException(typeof(MismatchedCallbackException))]
    public async Task ProcessCallbackAsync_Throws_WhenRequestIsUntracked()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, "fe79c213-9e22-4ba9-8a53-a711f98603c0");

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_StopsTrackingRequestId()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Cancel);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationRequestTrackingProvider>(mock =>
            mock.SetTrackedRequestAsync(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<Guid?>(requestId => requestId == null)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenEncountersUnexpectedCallbackType()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, "unexpected");

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => selector.ProcessCallbackAsync(fakeContext.Object, default)
        );
        Assert.AreEqual("Unexpected callback type 'unexpected'.", exception.Message);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnCancelSelection_WhenCancelledByUser()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Cancel);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnCancelSelection(
                It.Is<IHttpContext>(context => context == fakeContext.Object)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnError_WhenErrorOccurs()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Error);
        MockQueryParam(autoMocker, CallbackParamNames.ErrorCode, SelectOrganisationErrorCode.InvalidSelection);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnError(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<string>(errorCode => errorCode == SelectOrganisationErrorCode.InvalidSelection)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnConfirmSelection_WhenOrganisationDetailsWereConfirmed()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Selection);
        MockQueryParam(autoMocker, CallbackParamNames.Selection, FakeOrganisationId);

        autoMocker.GetMock<IInteractor<QueryUserOrganisation_PublicApiRequest, QueryUserOrganisation_PublicApiResponse>>()
            .Setup(mock =>
                mock.InvokeAsync(
                    It.Is<QueryUserOrganisation_PublicApiRequest>(request =>
                        request.OrganisationId == new Guid(FakeOrganisationId) &&
                        request.UserId == FakeUserId
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new QueryUserOrganisation_PublicApiResponse {
                Organisation = FakeOrganisation,
                UserId = FakeUserId,
            });

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnConfirmSelection(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<OrganisationDetails?>(organisation => organisation == FakeOrganisation)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnError_WhenSelectionWasNotBeConfirmed()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Selection);
        MockQueryParam(autoMocker, CallbackParamNames.Selection, FakeOrganisationId);

        autoMocker.GetMock<IInteractor<QueryUserOrganisation_PublicApiRequest, QueryUserOrganisation_PublicApiResponse>>()
            .Setup(mock =>
                mock.InvokeAsync(
                    It.IsAny<QueryUserOrganisation_PublicApiRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new QueryUserOrganisation_PublicApiResponse {
                Organisation = null,
                UserId = FakeUserId,
            });

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnError(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<string>(errorCode => errorCode == SelectOrganisationErrorCode.InvalidSelection)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnError_WhenUserIdDoesNotMatchConfirmationResponse()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.Selection);
        MockQueryParam(autoMocker, CallbackParamNames.Selection, FakeOrganisationId);

        autoMocker.GetMock<IInteractor<QueryUserOrganisation_PublicApiRequest, QueryUserOrganisation_PublicApiResponse>>()
            .Setup(mock =>
                mock.InvokeAsync(
                    It.IsAny<QueryUserOrganisation_PublicApiRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new QueryUserOrganisation_PublicApiResponse {
                Organisation = FakeOrganisation,
                UserId = new Guid("ac62be99-b4fe-4b19-9a3b-884a3c15b860"),
            });

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnError(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<string>(errorCode => errorCode == SelectOrganisationErrorCode.InvalidSelection)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_RaisesEvent_OnSignOut_WhenUserHasRequestedToSignOut()
    {
        var autoMocker = new AutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid(FakeRequestId));
        var fakeContext = SetupMockContext(autoMocker);
        SetupMockAuthenticatedUser(autoMocker);

        MockQueryParam(autoMocker, CallbackParamNames.RequestId, FakeRequestId);
        MockQueryParam(autoMocker, CallbackParamNames.Type, CallbackTypes.SignOut);

        var selector = autoMocker.CreateInstance<StandardSelectOrganisationUserFlow>();

        await selector.ProcessCallbackAsync(fakeContext.Object, default);

        autoMocker.Verify<ISelectOrganisationEvents>(mock =>
            mock.OnSignOut(
                It.Is<IHttpContext>(context => context == fakeContext.Object)
            ),
            Times.Once
        );
    }

    #endregion
}
