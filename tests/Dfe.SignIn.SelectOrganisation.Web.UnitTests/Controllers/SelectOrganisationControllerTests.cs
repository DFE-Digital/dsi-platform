using System.Text.Json;
using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.Organisations.Interactions;
using Dfe.SignIn.Core.Models.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Controllers;
using Dfe.SignIn.SelectOrganisation.Web.Models;
using Dfe.SignIn.SelectOrganisation.Web.UnitTests.TestHelpers;
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
        Status = 1,
    };

    private static readonly OrganisationModel FakeOrganisationB = new() {
        Id = new Guid("8498beb4-e3a4-4a1b-93c6-462084d84ab5"),
        Name = "Fake Organisation B",
        LegalName = "Legal Organisation B",
        Status = 1,
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

        autoMocker.Use(
            new MapperConfiguration(cfg => {
                cfg.AddProfile<SelectedOrganisationCallbackMappingProfile>();
            }).CreateMapper()
        );

        autoMocker.GetMock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationByClientIdRequest>(param => param.ClientId == "invalid-client")
            ))
            .ReturnsAsync(new GetApplicationByClientIdResponse { Application = null });

        autoMocker.GetMock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationByClientIdRequest>(param => param.ClientId == "mock-client")
            ))
            .ReturnsAsync(new GetApplicationByClientIdResponse { Application = FakeApplication });

        autoMocker.GetMock<IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<CreateDigitalSignatureForPayloadRequest>()
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
                    It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == organisation.Id)
                ))
                .ReturnsAsync(new GetOrganisationByIdResponse { Organisation = organisation });
        }

        return autoMocker;
    }

    private static void MockSessionNotFound(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<GetSelectOrganisationSessionByKeyRequest>()))
            .ReturnsAsync(new GetSelectOrganisationSessionByKeyResponse {
                SessionData = null,
            });
    }

    private static void MockSession(AutoMocker autoMocker, SelectOrganisationSessionData sessionData)
    {
        autoMocker.GetMock<IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<GetSelectOrganisationSessionByKeyRequest>()))
            .ReturnsAsync(new GetSelectOrganisationSessionByKeyResponse {
                SessionData = sessionData
            });
    }

    #region Index(string, string)

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
            new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
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
                )
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
                It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == new Guid("1d219e73-c674-4f8c-b982-d673ab02f015"))
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
            new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
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
            new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
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

        var result = await controller.Index("mock-client", "091889d2-1210-4dc0-8cec-be7975598916");

        var viewModel = TypeAssert.IsViewModelType<SelectOrganisationViewModel>(result);
        Assert.AreEqual("Which organisation would you like to contact?", viewModel.Prompt.Heading);
        Assert.AreEqual("Select one option.", viewModel.Prompt.Hint);

        var expectedOptions = FakeSessionWithMultipleOptions.OrganisationOptions.ToArray();
        CollectionAssert.AreEqual(expectedOptions, viewModel.OrganisationOptions.ToArray());
    }

    #endregion

    #region Index(string, string)

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
    public async Task PostIndex_InvalidateSession()
    {
        var autoMocker = CreateAutoMocker();
        MockSession(autoMocker, FakeSessionWithOneOption);
        var controller = autoMocker.CreateInstance<SelectOrganisationController>();

        var inputViewModel = Activator.CreateInstance<SelectOrganisationViewModel>();

        await controller.PostIndex("mock-client", "091889d2-1210-4dc0-8cec-be7975598916", inputViewModel);

        autoMocker.Verify<IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>>(
            x => x.InvokeAsync(
                It.Is<InvalidateSelectOrganisationSessionRequest>(
                    request => request.SessionKey == "091889d2-1210-4dc0-8cec-be7975598916"
                )
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
                It.Is<GetOrganisationByIdRequest>(param => param.OrganisationId == new Guid("1d219e73-c674-4f8c-b982-d673ab02f015"))
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
            new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
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
            new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        )!;
        Assert.AreEqual(PayloadTypeConstants.Id, callbackData.Type);
        Assert.AreEqual(new Guid("3c44b79a-991f-4068-b8d9-a761d651146f"), callbackData.Id);
    }

    #endregion
}
