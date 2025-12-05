using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Features;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangeEmailControllerTests
{
    private static ChangeEmailController CreateController(AutoMocker autoMocker, HttpContext httpContext)
    {
        autoMocker.GetMock<IOptionsMonitor<ApplicationOidcOptions>>()
            .Setup(x => x.CurrentValue)
            .Returns(new ApplicationOidcOptions {
                ClientId = "test",
            });

        autoMocker.GetMock<IOptionsMonitor<DistributedCacheInteractionLimiterOptions>>()
            .Setup(x => x.Get(nameof(InitiateChangeEmailAddressRequest)))
            .Returns(new DistributedCacheInteractionLimiterOptions {
                InteractionsPerTimePeriod = 4,
                TimePeriodInSeconds = 10,
            });

        var controller = autoMocker.CreateInstance<ChangeEmailController>();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = autoMocker.CreateInstance<TempDataDictionary>();

        return controller;
    }

    private static ChangeEmailController CreateControllerAuthenticated(AutoMocker autoMocker)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = false,
            IsInternalUser = false,
            GivenName = "Alex",
            Surname = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ], "TestAuth"));

        return CreateController(autoMocker, httpContext);
    }

    private static ChangeEmailController CreateControllerAnonymous(AutoMocker autoMocker)
    {
        return CreateController(autoMocker, new DefaultHttpContext());
    }

    private static void SetupFakePendingEmailChange(AutoMocker autoMocker)
    {
        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            },
            new GetPendingChangeEmailAddressResponse {
                PendingChangeEmailAddress = new() {
                    UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                    NewEmailAddress = "alex.new@example.com",
                    VerificationCode = "ABC1234",
                    ExpiryTimeUtc = new DateTime(2025, 11, 15, 12, 43, 11, DateTimeKind.Utc),
                    HasExpired = false,
                },
            }
        );
    }

    #region Index()

    [TestMethod]
    public void Index_PresentsExpectedView()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    #endregion

    #region PostIndex(bool?, ChangeEmailViewModel)

    [TestMethod]
    public async Task PostIndex_ReturnsFormView_WhenModelStateIsInvalid()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        controller.ModelState.AddModelError("", "Fake error.");

        var result = await controller.PostIndex(resend: false, viewModel: new());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostIndex_SendsExpectedInitiateChangeEmailAddressRequest()
    {
        var autoMocker = new AutoMocker();

        InitiateChangeEmailAddressRequest? capturedRequest = null;
        autoMocker.CaptureRequest<InitiateChangeEmailAddressRequest>(req => capturedRequest = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.PostIndex(resend: true, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("test", capturedRequest.ClientId);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedRequest.UserId);
        Assert.IsTrue(capturedRequest.IsSelfInvoked);
    }

    [TestMethod]
    public async Task PostIndex_SetsFlashNotification_WhenResendQueryParameterIsTrue()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        await controller.PostIndex(resend: true, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual("Verification code resent", flashNotification.Heading);
        Assert.Contains("We have sent an account verification email to alex.new@example.com.", flashNotification.Message);
    }

    [TestMethod]
    public async Task PostIndex_DoesNotSetFlashNotification_WhenResendQueryParameterIsFalse()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        await controller.PostIndex(resend: false, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNull(flashNotification);
    }

    [TestMethod]
    public async Task PostIndex_DoesNotHideResend()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        await controller.PostIndex(resend: false, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        bool hideResend = (bool)controller.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey]!;
        Assert.IsFalse(hideResend);
    }

    [TestMethod]
    public async Task PostIndex_SetsFlashNotification_WhenInteractionRejectedByLimiter_AndPendingEmailChange()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<InitiateChangeEmailAddressRequest>(
            new InteractionRejectedByLimiterException()
        );

        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            },
            new GetPendingChangeEmailAddressResponse {
                PendingChangeEmailAddress = new() {
                    UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                    NewEmailAddress = "alex.new@example.com",
                    VerificationCode = "ABC1234",
                    ExpiryTimeUtc = new DateTime(2025, 11, 15, 12, 43, 11, DateTimeKind.Utc),
                    HasExpired = false,
                },
            }
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.PostIndex(resend: true, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual("Verification code limit reached", flashNotification.Heading);
        Assert.Contains("For security, only 4 verification code requests can be sent.", flashNotification.Message);
        Assert.Contains("Wait 10 seconds before raising another request, or enter your verification code below.", flashNotification.Message);

        bool hideResend = (bool)controller.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey]!;
        Assert.IsTrue(hideResend);
    }

    [TestMethod]
    public async Task PostIndex_SetsFlashNotification_WhenInteractionRejectedByLimiter_AndNoPendingEmailChange()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<InitiateChangeEmailAddressRequest>(
            new InteractionRejectedByLimiterException()
        );

        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            },
            new GetPendingChangeEmailAddressResponse()
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.PostIndex(resend: true, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual("Verification code limit reached", flashNotification.Heading);
        Assert.Contains("For security, only 4 verification code requests can be sent.", flashNotification.Message);
        Assert.Contains("Wait 10 seconds before trying again.", flashNotification.Message);

        bool hideResend = (bool)controller.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey]!;
        Assert.IsTrue(hideResend);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToVerificationCodeForm()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.PostIndex(resend: true, viewModel: new() {
            EmailAddressInput = "alex.new@example.com",
        });

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(ChangeEmailController.VerificationCode), redirectResult.ActionName);
    }

    #endregion

    #region VerificationCode(CancellationToken)

    [TestMethod]
    public async Task VerificationCode_RedirectsHomeWhenNoPendingChange()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            },
            new GetPendingChangeEmailAddressResponse()
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.VerificationCode();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task VerificationCode_BadRequest_WhenModelStateIsInvalid()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        controller.ModelState.AddModelError("", "Fake error.");

        var result = await controller.VerificationCode();

        TypeAssert.IsType<BadRequestResult>(result);
    }

    [TestMethod]
    public async Task VerificationCode_ClearsAnyPreviousInputs()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAuthenticated(autoMocker);

        controller.ModelState.SetModelValue(nameof(VerificationCodeViewModel.VerificationCodeInput), "abc123", "abc123");
        controller.ModelState.MarkFieldValid(nameof(VerificationCodeViewModel.VerificationCodeInput));

        await controller.VerificationCode();

        var currentInputState = controller.ModelState[nameof(VerificationCodeViewModel.VerificationCodeInput)];
        Assert.IsNull(currentInputState!.RawValue);
        Assert.AreEqual("", currentInputState!.AttemptedValue);
    }

    [TestMethod]
    public async Task VerificationCode_PresentsExpectedView()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.VerificationCode();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("VerificationCode", viewResult.ViewName);

        var viewModel = TypeAssert.IsType<VerificationCodeViewModel>(viewResult.Model);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), viewModel.UserId);
        Assert.AreEqual("alex.new@example.com", viewModel.NewEmailAddress);
    }

    #endregion

    #region VerificationCodeAnonymous(Guid, CancellationToken)

    [TestMethod]
    public async Task VerificationCodeAnonymous_RedirectsHome_WhenNoPendingChange()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            },
            new GetPendingChangeEmailAddressResponse()
        );

        var controller = CreateControllerAnonymous(autoMocker);

        var result = await controller.VerificationCodeAnonymous(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"));

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task VerificationCodeAnonymous_BadRequest_WhenModelStateIsInvalid()
    {
        var controller = CreateControllerAnonymous(new AutoMocker());

        controller.ModelState.AddModelError("", "Fake error.");

        var result = await controller.VerificationCodeAnonymous(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"));

        TypeAssert.IsType<BadRequestResult>(result);
    }

    [TestMethod]
    public async Task VerificationCodeAnonymous_ClearsAnyPreviousInputs()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAnonymous(autoMocker);

        controller.ModelState.SetModelValue(nameof(VerificationCodeViewModel.VerificationCodeInput), "abc123", "abc123");
        controller.ModelState.MarkFieldValid(nameof(VerificationCodeViewModel.VerificationCodeInput));

        await controller.VerificationCodeAnonymous(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"));

        var currentInputState = controller.ModelState[nameof(VerificationCodeViewModel.VerificationCodeInput)];
        Assert.IsNull(currentInputState!.RawValue);
        Assert.AreEqual("", currentInputState!.AttemptedValue);
    }

    [TestMethod]
    public async Task VerificationCodeAnonymous_PresentsExpectedView()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAnonymous(autoMocker);

        var result = await controller.VerificationCodeAnonymous(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"));

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("VerificationCode", viewResult.ViewName);

        var viewModel = TypeAssert.IsType<VerificationCodeViewModel>(viewResult.Model);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), viewModel.UserId);
        Assert.AreEqual("alex.new@example.com", viewModel.NewEmailAddress);
    }

    #endregion

    #region PostVerificationCode(Guid, VerificationCodeViewModel, CancellationToken)

    [TestMethod]
    public async Task PostVerificationCode_RedirectsToChangeEmailForm_WhenNoPendingChange()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<ConfirmChangeEmailAddressRequest>(
            new NoPendingChangeEmailException()
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.PostVerificationCode(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            new VerificationCodeViewModel {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                NewEmailAddress = "alex.new@example.com",
            }
        );

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(ChangeEmailController.Index), redirectResult.ActionName);
    }

    [TestMethod]
    public async Task PostVerificationCode_PresentsFormToVerifyCode_WhenModelStateInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAuthenticated(autoMocker);

        controller.ModelState.AddModelError("", "Fake input error.");

        var result = await controller.PostVerificationCode(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            new VerificationCodeViewModel {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                NewEmailAddress = "alex.new@example.com",
            }
        );

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("VerificationCode", viewResult.ViewName);

        var viewModel = TypeAssert.IsType<VerificationCodeViewModel>(viewResult.Model);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), viewModel.UserId);
        Assert.AreEqual("alex.new@example.com", viewModel.NewEmailAddress);
    }

    [TestMethod]
    public async Task PostVerificationCode_RedirectsToComplete_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        SetupFakePendingEmailChange(autoMocker);
        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.PostVerificationCode(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            new VerificationCodeViewModel {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                NewEmailAddress = "alex.new@example.com",
            }
        );

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(ChangeEmailController.Complete), redirectResult.ActionName);
    }

    [TestMethod]
    public async Task PostVerificationCode_PresentsError_WhenFailedToUpdateAuthenticationMethod()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<ConfirmChangeEmailAddressRequest>(
            new FailedToUpdateAuthenticationMethodException()
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.PostVerificationCode(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            new VerificationCodeViewModel {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                NewEmailAddress = "alex.new@example.com",
            }
        );

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("ErrorUpdateAuthenticationMethod", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostVerificationCode_PresentsError_WhenUnexpectedFailureOccurs()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<ConfirmChangeEmailAddressRequest>(
            new InvalidOperationException()
        );

        var controller = CreateControllerAuthenticated(autoMocker);

        var result = await controller.PostVerificationCode(
            new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            new VerificationCodeViewModel {
                UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                NewEmailAddress = "alex.new@example.com",
            }
        );

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("ErrorUpdateEmailAddress", viewResult.ViewName);
    }

    #endregion

    #region Complete()

    [TestMethod]
    public void Complete_PresentsExpectedView()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = controller.Complete();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Complete", viewResult.ViewName);
    }

    #endregion

    #region PostComplete()

    [TestMethod]
    public void PostComplete_FlashCancelled()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        controller.PostComplete();

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Success, flashNotification.Type);
        Assert.AreEqual("Email address updated successfully", flashNotification.Heading);
        Assert.AreEqual("Please allow up to 10 minutes for the changes to take effect before signing back into your account.", flashNotification.Message);
    }

    [TestMethod]
    public void PostComplete_RedirectsToHome()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = controller.PostComplete();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion

    #region PostCancel()

    [TestMethod]
    public async Task PostCancel_CancelsPendingChange()
    {
        var autoMocker = new AutoMocker();

        CancelPendingChangeEmailAddressRequest? capturedRequest = null;
        autoMocker.CaptureRequest<CancelPendingChangeEmailAddressRequest>(req => capturedRequest = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.PostCancel();

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedRequest.UserId);
    }

    [TestMethod]
    public async Task PostCancel_FlashCancelled()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        await controller.PostCancel();

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Default, flashNotification.Type);
        Assert.AreEqual("Email change cancelled", flashNotification.Heading);
        Assert.AreEqual("As you did not complete the email change process, your email change has been cancelled.", flashNotification.Message);
    }

    [TestMethod]
    public async Task PostCancel_RedirectsToHome()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.PostCancel();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion
}
