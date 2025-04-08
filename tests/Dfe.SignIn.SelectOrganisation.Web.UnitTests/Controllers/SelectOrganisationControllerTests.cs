using System.Text;
using System.Text.Json;
using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Applications;
using Dfe.SignIn.Core.InternalModels.Applications.Interactions;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations.Interactions;
using Dfe.SignIn.Core.InternalModels.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Controllers;
using Dfe.SignIn.SelectOrganisation.Web.Models;
using Dfe.SignIn.SelectOrganisation.Web.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.Controllers;

[TestClass]
public sealed class SelectOrganisationControllerTests
{
    private static readonly Guid FakeUserId = new();

    private static readonly OrganisationModel FakeOrganisationA = new() {
        Id = new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"),
        Name = "Fake Organisation A",
        LegalName = "Legal Organisation A",
        Status = OrganisationStatus.Open,
    };

    private static readonly OrganisationModel FakeOrganisationB = new() {
        Id = new Guid("8498beb4-e3a4-4a1b-93c6-462084d84ab5"),
        Name = "Fake Organisation B",
        LegalName = "Legal Organisation B",
        Status = OrganisationStatus.Open,
    };

    private static readonly ApplicationModel FakeApplication = new() {
        ClientId = "mock-client",
        Id = new Guid("46fc7902-450b-445c-8565-2dbb8c1d8c84"),
        IsExternalService = true,
        IsHiddenService = true,
        IsIdOnlyService = false,
        Name = "Mock Application",
        ServiceHomeUrl = new Uri("https://mock-service.localhost"),
    };

    private static readonly SelectOrganisationSessionData FakeSessionWithNoOptions = new() {
        Created = DateTime.UtcNow,
        Expires = DateTime.UtcNow + new TimeSpan(0, 10, 0),
        ClientId = "mock-client",
        UserId = FakeUserId,
        AllowCancel = true,
        CallbackUrl = new Uri("http://mock-service.localhost/callback"),
        DetailLevel = OrganisationDetailLevel.Id,
        OrganisationOptions = [],
        Prompt = new() {
            Heading = "Which organisation would you like to contact?",
            Hint = "Select one option.",
        },
    };

    private static readonly SelectOrganisationSessionData FakeSessionWithOneOption = FakeSessionWithNoOptions with {
        OrganisationOptions = [
            new() {
                Id = new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"),
                Name = "Fake Organisation A",
            },
        ],
    };

    private static readonly SelectOrganisationSessionData FakeSessionWithOneOptionWhereOrganisationDoesNotExist = FakeSessionWithNoOptions with {
        OrganisationOptions = [
            new() {
                Id = new Guid("1d219e73-c674-4f8c-b982-d673ab02f015"),
                Name = "Fake Non-Existent Organisation",
            },
        ],
    };

    private static readonly SelectOrganisationSessionData FakeSessionWithMultipleOptions = FakeSessionWithNoOptions with {
        OrganisationOptions = [
            new() {
                Id = new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"),
                Name = "Fake Organisation A",
            },
            new() {
                Id = new Guid("8498beb4-e3a4-4a1b-93c6-462084d84ab5"),
                Name = "Fake Organisation B",
            },
        ],
    };

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IOptions<ApplicationOptions>>()
            .Setup(x => x.Value)
            .Returns(new ApplicationOptions {
                ServicesUrl = new Uri("https://services.localhost"),
            });

        autoMocker.Use(JsonHelperExtensions.CreateStandardOptions());

        autoMocker.Use(
            new MapperConfiguration(cfg => {
                cfg.AddProfile<SelectedOrganisationCallbackMappingProfile>();
            }).CreateMapper()
        );

        autoMocker.GetMock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationByClientIdRequest>(param => param.ClientId == "invalid-client"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetApplicationByClientIdResponse { Application = null });

        autoMocker.GetMock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationByClientIdRequest>(param => param.ClientId == "mock-client"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetApplicationByClientIdResponse { Application = FakeApplication });

        autoMocker.GetMock<IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<CreateDigitalSignatureForPayloadRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new CreateDigitalSignatureForPayloadResponse {
                Signature = new() {
                    KeyId = "FakePublicKey1",
                    Signature = "FakeSignatureXyz",
                }
            });

        foreach (var organisation in new[] { FakeOrganisationA, FakeOrganisationB }) {
            autoMocker.GetMock<IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>>()
                .Setup(x => x.InvokeAsync(
                    It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == organisation.Id),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new GetOrganisationByIdResponse { Organisation = organisation });
        }

        return autoMocker;
    }

    private static void MockSessionNotFound(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<GetSelectOrganisationSessionByKeyRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetSelectOrganisationSessionByKeyResponse {
                SessionData = null,
            });
    }

    private static void MockSession(AutoMocker autoMocker, SelectOrganisationSessionData sessionData)
    {
        autoMocker.GetMock<IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<GetSelectOrganisationSessionByKeyRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetSelectOrganisationSessionByKeyResponse {
                SessionData = sessionData
            });
    }

    private static IUrlHelper CreateMockUrlHelper()
    {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/sign-out");
        return mockUrlHelper.Object;
    }

    #region Index(string, string, CancellationToken)

    [TestMethod]
    public async Task Index_RedirectsToInvalidSessionHandler_WithApplicationServicesUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("invalid-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://services.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task Index_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task Index_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenUserHasTamperedWithClientIdParameter()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithNoOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("tampered-client-id", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task Index_SendsErrorCallback_WhenUserHasNoOptions()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithNoOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Error, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var error = JsonSerializer.Deserialize<SelectOrganisationCallbackError>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Error, error.Type);
        Assert.AreEqual(SelectOrganisationErrorCode.NoOptions, error.Code);
    }

    [TestMethod]
    public async Task Index_InvalidateSession_WhenUserHasOneOption()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOption);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        autoMocker.Verify<IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>>(
            x => x.InvokeAsync(
                It.Is<InvalidateSelectOrganisationSessionRequest>(
                    request => request.SessionKey == "091889d2-1210-4dc0-8cec-be7975598916"
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [TestMethod]
    public async Task Index_SendsErrorCallback_WhenUserHasOneOptionButOrganisationDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOptionWhereOrganisationDoesNotExist);

        autoMocker.GetMock<IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == new Guid("1d219e73-c674-4f8c-b982-d673ab02f015")),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetOrganisationByIdResponse { Organisation = null });

        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Error, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var error = JsonSerializer.Deserialize<SelectOrganisationCallbackError>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Error, error.Type);
        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, error.Code);
    }

    [TestMethod]
    public async Task Index_SendsCallback_WhenUserHasOneOption()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOption);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Id, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var callbackData = JsonSerializer.Deserialize<SelectOrganisationCallbackId>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Id, callbackData.Type);
        Assert.AreEqual(new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"), callbackData.Id);
    }

    [TestMethod]
    public async Task Index_PresentsOptionsToUser_WhenUserHasMultipleOptions()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithMultipleOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();
        controller.Url = CreateMockUrlHelper();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<SelectOrganisationViewModel>(result);
        Assert.AreEqual("Which organisation would you like to contact?", viewModel.Prompt.Heading);
        Assert.AreEqual("Select one option.", viewModel.Prompt.Hint);

        var expectedOptions = FakeSessionWithMultipleOptions.OrganisationOptions.ToArray();
        CollectionAssert.AreEqual(expectedOptions, viewModel.OrganisationOptions.ToArray());
    }

    [DataRow(true)]
    [DataRow(false)]
    [DataTestMethod]
    public async Task Index_PresentsReflectsSessionAllowCancel(bool allowCancel)
    {
        var autoMocker = CreateAutoMocker();

        MockSession(autoMocker, FakeSessionWithMultipleOptions with {
            AllowCancel = allowCancel,
        });

        var controller = autoMocker.CreateInstance<SelectOrganisationController>();
        controller.Url = CreateMockUrlHelper();

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<SelectOrganisationViewModel>(result);
        Assert.AreEqual(allowCancel, viewModel.AllowCancel);
    }

    #endregion

    #region Index(string, string, CancellationToken)

    [TestMethod]
    public async Task PostIndex_RedirectsToInvalidSessionHandler_WithApplicationServicesUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();

        var result = await controller.PostIndex("invalid-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://services.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenUserHasTamperedWithClientIdParameter()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithNoOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();

        var result = await controller.PostIndex("tampered-client-id", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task PostIndex_DoesNotSendCancelCallback_WhenCancelActionSetAndCancelNotAllowed()
    {
        var autoMocker = CreateAutoMocker();
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        MockSession(autoMocker, FakeSessionWithMultipleOptions with {
            AllowCancel = false,
        });

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.Cancel = "1";

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Error, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var callbackData = JsonSerializer.Deserialize<SelectOrganisationCallbackError>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Error, callbackData.Type);
        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, callbackData.Code);
    }

    [TestMethod]
    public async Task PostIndex_SendsCancelCallback_WhenCancelActionSetAndCancelAllowed()
    {
        var autoMocker = CreateAutoMocker();
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        MockSession(autoMocker, FakeSessionWithMultipleOptions with {
            AllowCancel = true,
        });

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.Cancel = "1";

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Cancel, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var callbackData = JsonSerializer.Deserialize<SelectOrganisationCallbackCancel>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Cancel, callbackData.Type);
    }

    [TestMethod]
    public async Task PostIndex_PresentsViewWithError_WhenNoOrganisationWasSelected()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOption);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();
        controller.Url = CreateMockUrlHelper();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = null;

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        Assert.AreEqual(1, controller.ModelState.ErrorCount);

        var viewModel = TypeAssert.IsViewModelType<SelectOrganisationViewModel>(result);
        Assert.AreEqual("Which organisation would you like to contact?", viewModel.Prompt.Heading);
        Assert.AreEqual("Select one option.", viewModel.Prompt.Hint);

        var expectedOptions = FakeSessionWithOneOption.OrganisationOptions.ToArray();
        CollectionAssert.AreEqual(expectedOptions, viewModel.OrganisationOptions.ToArray());
    }

    [TestMethod]
    public async Task PostIndex_InvalidateSession()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOption);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();
        controller.Url = CreateMockUrlHelper();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = new Guid("b8f142c3-b853-4a9b-8d79-c53c33f6d7b4");

        await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        autoMocker.Verify<IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>>(
            x => x.InvokeAsync(
                It.Is<InvalidateSelectOrganisationSessionRequest>(
                    request => request.SessionKey == "091889d2-1210-4dc0-8cec-be7975598916"
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenUserSelectsOptionThatWasNotPresented()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithMultipleOptions);

        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = new Guid("51a8b477-f92b-41af-a86a-91ce26037c14");

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task PostIndex_SendsErrorCallback_WhenUserSelectsOptionButOrganisationDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOptionWhereOrganisationDoesNotExist);

        autoMocker.GetMock<IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == new Guid("1d219e73-c674-4f8c-b982-d673ab02f015")),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetOrganisationByIdResponse { Organisation = null });

        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = new Guid("1d219e73-c674-4f8c-b982-d673ab02f015");

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Error, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var error = JsonSerializer.Deserialize<SelectOrganisationCallbackError>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Error, error.Type);
        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, error.Code);
    }

    [TestMethod]
    public async Task PostIndex_SendsCallback_WhenUserSelectsOption()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithMultipleOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = new Guid("3c44b79a-991f-4068-b8d9-a761d651146f");

        var result = await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.Id, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var callbackData = JsonSerializer.Deserialize<SelectOrganisationCallbackId>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.Id, callbackData.Type);
        Assert.AreEqual(new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"), callbackData.Id);
    }

    [TestMethod]
    public async Task PostIndex_SignsExpectedPayload()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithMultipleOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();
        inputViewModel.SelectedOrganisationId = new Guid("3c44b79a-991f-4068-b8d9-a761d651146f");

        await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        var expectedPayload = new SelectOrganisationCallbackId {
            Type = PayloadTypeConstants.Id,
            Id = (Guid)inputViewModel.SelectedOrganisationId,
        };
        var expectedPayloadJson = JsonSerializer.Serialize(
            expectedPayload,
            JsonHelperExtensions.CreateStandardOptions()
        );
        string expectedPayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayloadJson));

        autoMocker.Verify<
            IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse>
        >(x =>
            x.InvokeAsync(
                It.Is<CreateDigitalSignatureForPayloadRequest>(
                    request => request.Payload == expectedPayloadBase64
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    #endregion

    #region SignOut(string, string, CancellationToken)

    [TestMethod]
    public async Task SignOut_RedirectsToInvalidSessionHandler_WithApplicationServicesUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.SignOut("invalid-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://services.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task SignOut_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenSessionDoesNotExist()
    {
        var autoMocker = CreateAutoMocker();
        MockSessionNotFound(autoMocker);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.SignOut("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task SignOut_RedirectsToInvalidSessionHandler_WithServiceHomeUrl_WhenUserHasTamperedWithClientIdParameter()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithNoOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.SignOut("tampered-client-id", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<InvalidSessionViewModel>(result);
        Assert.AreEqual(new Uri("https://mock-service.localhost"), viewModel.ReturnUrl);
    }

    [TestMethod]
    public async Task SignOut_SendsSignOutCallback()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithMultipleOptions);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var result = await controller.SignOut("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<CallbackViewModel>(result);
        Assert.AreEqual(new Uri("http://mock-service.localhost/callback"), viewModel.CallbackUrl);
        Assert.AreEqual(PayloadTypeConstants.SignOut, viewModel.PayloadType);
        Assert.AreEqual("FakeSignatureXyz", viewModel.DigitalSignature);
        Assert.AreEqual("FakePublicKey1", viewModel.PublicKeyId);

        var callbackData = JsonSerializer.Deserialize<SelectOrganisationCallbackSignOut>(
            Convert.FromBase64String(viewModel.PayloadData),
            JsonHelperExtensions.CreateStandardOptions()
        )!;
        Assert.AreEqual(PayloadTypeConstants.SignOut, callbackData.Type);
    }

    #endregion
}
