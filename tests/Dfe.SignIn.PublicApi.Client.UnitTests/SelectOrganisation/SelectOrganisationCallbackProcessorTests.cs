using System.Security.Claims;
using System.Text;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackProcessorTests
{
    private static readonly Guid FakeUserId1 = new("3852c1fa-a57f-492d-96f9-500ba5a9a20e");
    private static readonly Guid FakeUserId2 = new("6c553f8f-4a1a-4f82-a99e-8d9949e07e78");
    private static readonly Guid FakeRequestId = new("0309b700-ce09-4ccf-8e24-05a48304f906");

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        var jsonSerializerOptions = autoMocker.UseStandardJsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new SelectOrganisationCallbackSelectionJsonConverter());

        return autoMocker;
    }

    private static Mock<IHttpContext> SetupMockContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakePrimaryIdentity = new ClaimsIdentity([
            new(DsiClaimTypes.UserId, FakeUserId1.ToString()),
        ], "PrimaryAuthenticationType");

        var fakeUser = new ClaimsPrincipal(fakePrimaryIdentity);
        mockContext.Setup(mock => mock.User)
            .Returns(fakeUser);

        mockContext.Setup(mock => mock.Request)
            .Returns(autoMocker.GetMock<IHttpRequest>().Object);

        autoMocker.GetMock<IHttpRequest>()
            .Setup(mock => mock.GetQuery(
                It.Is<string>(key => key == "rid")
            ))
            .Returns(FakeRequestId.ToString());

        return mockContext;
    }

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

    #region ProcessCallbackAsync(SelectOrganisationCallbackViewModel, bool, CancellationToken)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_Throws_WhenViewModelArgumentIsNull()
    {
        var autoMocker = CreateAutoMocker();
        var fakeContext = SetupMockContext(autoMocker);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel: null!,
            throwOnError: false
        );
    }

    [TestMethod]
    [ExpectedException(typeof(MismatchedCallbackException))]
    public async Task ProcessCallbackAsync_Throws_WhenRequestIsUntracked()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, new Guid("85a5e33e-37fc-41f9-9c02-e1e610ea91ad"));
        var fakeContext = SetupMockContext(autoMocker);

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Selection,
            Payload = "fake payload",
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenPayloadCannotBeVerified()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Selection,
            Payload = "fake payload",
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.Is<string>(payload => payload == viewModel.Payload),
                It.Is<PayloadDigitalSignature>(signature =>
                    signature.KeyId == viewModel.Kid &&
                    signature.Signature == viewModel.Sig
                )
            ))
            .ReturnsAsync(false);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => processor.ProcessCallbackAsync(
                fakeContext.Object,
                viewModel,
                throwOnError: false
            )
        );
        Assert.AreEqual("Invalid payload.", exception.Message);
    }

    [TestMethod]
    [ExpectedException(typeof(MismatchedCallbackException))]
    public async Task ProcessCallbackAsync_Throws_WhenDoesNotCorrespondWithCallback()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
              "type": "error",
              "userId": "{{FakeUserId2}}",
              "code": "noOptions"
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Error,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayload)),
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.Is<string>(payload => payload == viewModel.Payload),
                It.Is<PayloadDigitalSignature>(signature =>
                    signature.KeyId == viewModel.Kid &&
                    signature.Signature == viewModel.Sig
                )
            ))
            .ReturnsAsync(true);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_ReturnsErrorCallbackData_WhenThrowOnErrorFalse()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
              "type": "error",
              "userId": "{{FakeUserId1}}",
              "code": "noOptions"
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Error,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayload)),
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.Is<string>(payload => payload == viewModel.Payload),
                It.Is<PayloadDigitalSignature>(signature =>
                    signature.KeyId == viewModel.Kid &&
                    signature.Signature == viewModel.Sig
                )
            ))
            .ReturnsAsync(true);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        var result = await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel,
            throwOnError: false
        );

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<SelectOrganisationCallbackError>(result);

        var expectedCallbackData = new SelectOrganisationCallbackError {
            Type = PayloadTypeConstants.Error,
            UserId = FakeUserId1,
            Code = SelectOrganisationErrorCode.NoOptions,
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenThrowOnErrorTrue()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Error,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                /*lang=json,strict*/ $$"""
                    {
                        "type": "error",
                        "userId": "{{FakeUserId1}}",
                        "code": "noOptions"
                    }
                """
            )),
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.Is<string>(payload => payload == viewModel.Payload),
                It.Is<PayloadDigitalSignature>(signature =>
                    signature.KeyId == viewModel.Kid &&
                    signature.Signature == viewModel.Sig
                )
            ))
            .ReturnsAsync(true);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        var exception = await Assert.ThrowsExceptionAsync<SelectOrganisationCallbackErrorException>(
            () => processor.ProcessCallbackAsync(
                fakeContext.Object,
                viewModel,
                throwOnError: true
            )
        );
        Assert.AreEqual(SelectOrganisationErrorCode.NoOptions, exception.ErrorCode);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_ReturnsPayloadData()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
                "type": "selection",
                "userId": "{{FakeUserId1}}",
                "detailLevel": "basic",
                "selection": {
                    "id": "80d5a7d9-0198-471f-9661-b618e8b9db15",
                    "name": "Example Organisation A",
                    "legalName": "Legal name of Example Organisation A"
                }
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Selection,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayload)),
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.Is<string>(payload => payload == viewModel.Payload),
                It.Is<PayloadDigitalSignature>(signature =>
                    signature.KeyId == viewModel.Kid &&
                    signature.Signature == viewModel.Sig
                )
            ))
            .ReturnsAsync(true);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        var result = await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel,
            throwOnError: true
        );

        var expectedCallbackData = new SelectOrganisationCallbackSelection {
            Type = PayloadTypeConstants.Selection,
            UserId = FakeUserId1,
            DetailLevel = OrganisationDetailLevel.Basic,
            Selection = new OrganisationDetailsBasic {
                Id = new Guid("80d5a7d9-0198-471f-9661-b618e8b9db15"),
                Name = "Example Organisation A",
                LegalName = "Legal name of Example Organisation A"
            },
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_StopsTrackingRequest()
    {
        var autoMocker = CreateAutoMocker();
        SetupMockRequestTracker(autoMocker, FakeRequestId);
        var fakeContext = SetupMockContext(autoMocker);

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
                "type": "cancel",
                "userId": "{{FakeUserId1}}"
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Cancel,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayload)),
            Sig = "fake signature",
            Kid = "80d5a7d9-0198-471f-9661-b618e8b9db15",
        };

        autoMocker.GetMock<IPayloadVerifier>()
            .Setup(mock => mock.VerifyPayload(
                It.IsAny<string>(),
                It.IsAny<PayloadDigitalSignature>()
            ))
            .ReturnsAsync(true);

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackAsync(
            fakeContext.Object,
            viewModel,
            throwOnError: false
        );

        autoMocker.Verify<ISelectOrganisationRequestTrackingProvider>(mock =>
            mock.SetTrackedRequestAsync(
                It.Is<IHttpContext>(context => context == fakeContext.Object),
                It.Is<Guid?>(requestId => requestId == null)
            ),
            Times.Once
        );
    }

    #endregion
}
