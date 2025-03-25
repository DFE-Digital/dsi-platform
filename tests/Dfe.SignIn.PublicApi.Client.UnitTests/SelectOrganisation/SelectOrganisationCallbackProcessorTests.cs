using System.Text;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackProcessorTests
{
    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        autoMocker.Use(JsonHelperExtensions.CreateStandardOptions());

        return autoMocker;
    }

    #region ProcessCallbackJsonAsync(SelectOrganisationCallbackViewModel, bool)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackJsonAsync_Throws_WhenViewModelArgumentIsNull()
    {
        var autoMocker = CreateAutoMocker();

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackJsonAsync(
            viewModel: null!,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackJsonAsync_Throws_WhenPayloadIsInvalid()
    {
        var autoMocker = CreateAutoMocker();

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Id,
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
            () => processor.ProcessCallbackJsonAsync(
                viewModel,
                throwOnError: false
            )
        );
        Assert.AreEqual("Invalid payload.", exception.Message);
    }

    [TestMethod]
    public async Task ProcessCallbackJsonAsync_ReturnsErrorCallbackData_WhenThrowOnErrorFalse()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            { "type": "error", "code": 2 }
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

        string result = await processor.ProcessCallbackJsonAsync(
            viewModel,
            throwOnError: false
        );

        Assert.AreEqual(expectedPayload, result);
    }

    [TestMethod]
    public async Task ProcessCallbackJsonAsync_Throws_WhenThrowOnErrorTrue()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            { "type": "error", "code": 2 }
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

        var exception = await Assert.ThrowsExceptionAsync<SelectOrganisationCallbackErrorException>(
            () => processor.ProcessCallbackJsonAsync(
                viewModel,
                throwOnError: true
            )
        );
        Assert.AreEqual(SelectOrganisationErrorCode.NoOptions, exception.ErrorCode);
    }

    [TestMethod]
    public async Task ProcessCallbackJsonAsync_ReturnsPayloadData()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
                "type": "id",
                "id": "80d5a7d9-0198-471f-9661-b618e8b9db15"
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Id,
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

        var result = await processor.ProcessCallbackJsonAsync(
            viewModel,
            throwOnError: true
        );

        Assert.AreEqual(expectedPayload, result);
    }

    #endregion

    #region ProcessCallbackRawAsync(SelectOrganisationCallbackViewModel, Type, bool)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackRawAsync_Throws_WhenViewModelArgumentIsNull()
    {
        var autoMocker = CreateAutoMocker();

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackRawAsync(
            viewModel: null!,
            targetType: typeof(SelectOrganisationCallbackBasic),
            throwOnError: false
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackRawAsync_Throws_WhenTargetTypeArgumentIsNull()
    {
        var autoMocker = CreateAutoMocker();

        var processor = autoMocker.CreateInstance<SelectOrganisationCallbackProcessor>();

        await processor.ProcessCallbackRawAsync(
            viewModel: Activator.CreateInstance<SelectOrganisationCallbackViewModel>(),
            targetType: null!,
            throwOnError: false
        );
    }

    [TestMethod]
    public async Task ProcessCallbackRawAsync_Throws_WhenPayloadIsInvalid()
    {
        var autoMocker = CreateAutoMocker();

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Id,
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
            () => processor.ProcessCallbackRawAsync(
                viewModel,
                targetType: typeof(SelectOrganisationCallbackId),
                throwOnError: false
            )
        );
        Assert.AreEqual("Invalid payload.", exception.Message);
    }

    [TestMethod]
    public async Task ProcessCallbackRawAsync_ReturnsErrorCallbackData_WhenThrowOnErrorFalse()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            { "type": "error", "code": 2 }
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

        var result = await processor.ProcessCallbackRawAsync(
            viewModel,
            targetType: typeof(SelectOrganisationCallbackId),
            throwOnError: false
        );

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<SelectOrganisationCallbackError>(result);

        var expectedCallbackData = new SelectOrganisationCallbackError {
            Type = PayloadTypeConstants.Error,
            Code = SelectOrganisationErrorCode.NoOptions,
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackRawAsync_Throws_WhenThrowOnErrorTrue()
    {
        var autoMocker = CreateAutoMocker();

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Error,
            Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                /*lang=json,strict*/ $$"""
                    { "type": "error", "code": 2 }
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
            () => processor.ProcessCallbackRawAsync(
                viewModel,
                targetType: typeof(SelectOrganisationCallbackId),
                throwOnError: true
            )
        );
        Assert.AreEqual(SelectOrganisationErrorCode.NoOptions, exception.ErrorCode);
    }

    [TestMethod]
    public async Task ProcessCallbackRawAsync_ReturnsPayloadData()
    {
        var autoMocker = CreateAutoMocker();

        string expectedPayload = /*lang=json,strict*/ $$"""
            {
                "type": "id",
                "id": "80d5a7d9-0198-471f-9661-b618e8b9db15"
            }
        """;

        var viewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Id,
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

        var result = await processor.ProcessCallbackRawAsync(
            viewModel,
            targetType: typeof(SelectOrganisationCallbackId),
            throwOnError: true
        );

        var expectedCallbackData = new SelectOrganisationCallbackId {
            Type = PayloadTypeConstants.Id,
            Id = new Guid("80d5a7d9-0198-471f-9661-b618e8b9db15"),
        };
        Assert.AreEqual(expectedCallbackData, result);
    }

    #endregion
}
