using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackProcessorTests
{
    private static readonly Guid FakeUserId = new("3852c1fa-a57f-492d-96f9-500ba5a9a20e");

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        var jsonSerializerOptions = JsonHelperExtensions.CreateStandardOptionsTestHelper();
        jsonSerializerOptions.Converters.Add(new SelectOrganisationCallbackSelectionJsonConverter());

        autoMocker.GetMock<IOptionsMonitor<JsonSerializerOptions>>()
            .Setup(mock => mock.Get(It.Is<string>(key => key == JsonHelperExtensions.StandardOptionsKey)))
            .Returns(jsonSerializerOptions);

        return autoMocker;
    }

    #region ProcessCallbackAsync(SelectOrganisationCallbackViewModel, bool, CancellationToken)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_Throws_WhenViewModelArgumentIsNull()
    {
        var autoMocker = CreateAutoMocker();

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackAsync(
            currentUserId: FakeUserId,
            viewModel: null!,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenPayloadCannotBeVerified()
    {
        var autoMocker = CreateAutoMocker();

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
                currentUserId: FakeUserId,
                viewModel,
                throwOnError: false
            )
        );
        Assert.AreEqual("Invalid payload.", exception.Message);
    }

    [TestMethod]
    [ExpectedException(typeof(MismatchedCallbackUserException))]
    public async Task ProcessCallbackAsync_Throws_WhenDoesNotCorrespondWithCallback()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
              "type": "error",
              "userId": "{{FakeUserId}}",
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
            currentUserId: new Guid("022b25eb-9b4e-4d73-b08d-c501330a3071"),
            viewModel,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_ReturnsErrorCallbackData_WhenThrowOnErrorFalse()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
              "type": "error",
              "userId": "{{FakeUserId}}",
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
            currentUserId: FakeUserId,
            viewModel,
            throwOnError: false
        );

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<SelectOrganisationCallbackError>(result);

        var expectedCallbackData = new SelectOrganisationCallbackError {
            Type = PayloadTypeConstants.Error,
            UserId = FakeUserId,
            Code = SelectOrganisationErrorCode.NoOptions,
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenThrowOnErrorTrue()
    {
        var autoMocker = CreateAutoMocker();

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Error,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                /*lang=json,strict*/ $$"""
                    {
                        "type": "error",
                        "userId": "{{FakeUserId}}",
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
                currentUserId: FakeUserId,
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

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
                "type": "selection",
                "userId": "{{FakeUserId}}",
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
            currentUserId: FakeUserId,
            viewModel,
            throwOnError: true
        );

        var expectedCallbackData = new SelectOrganisationCallbackSelection {
            Type = PayloadTypeConstants.Selection,
            UserId = FakeUserId,
            DetailLevel = OrganisationDetailLevel.Basic,
            Selection = new SelectedOrganisationBasic {
                Id = new Guid("80d5a7d9-0198-471f-9661-b618e8b9db15"),
                Name = "Example Organisation A",
                LegalName = "Legal name of Example Organisation A"
            },
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    #endregion
}
