using Dfe.SignIn.WebFramework.Mvc.Controllers;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class ControllerExtensionsTests
{
    private sealed class FakeErrorController : BaseErrorController { }

    #region ErrorView(Controller, string?, LegacyErrorViewModel?)

    [TestMethod]
    public void ErrorView_Throws_WhenControllerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerExtensions.ErrorView(null!));
    }

    [TestMethod]
    public void ErrorView_PresentsNamedView_WhenViewNameIsSpecified()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var result = ControllerExtensions.ErrorView(controller, "ExampleViewName");

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("ExampleViewName", viewResult.ViewName);
    }

    [TestMethod]
    public void ErrorView_PresentsEmptyViewModel_WhenModelArgumentIsNull()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var result = ControllerExtensions.ErrorView(controller);

        TypeAssert.IsViewModelType<LegacyErrorViewModel>(result);
    }

    [TestMethod]
    public void ErrorView_PresentsGivenViewModel_WhenModelArgumentIsSpecified()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var model = new LegacyErrorViewModel();

        var result = ControllerExtensions.ErrorView(controller, model: model);

        var actualModel = TypeAssert.IsViewModelType<LegacyErrorViewModel>(result);
        Assert.AreSame(model, actualModel);
    }

    [TestMethod]
    public void ErrorView_PresentsTraceIdentifierAsRequestId()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext {
                    TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090"
                },
            },
        };

        var result = ControllerExtensions.ErrorView(controller);

        var viewModel = TypeAssert.IsViewModelType<LegacyErrorViewModel>(result);
        Assert.AreEqual("a492f33c-a859-4098-8c01-b8b2f09a6090", viewModel.RequestId);
    }

    #endregion

    #region ErrorView(Controller, ErrorViewModel, string?)

    [TestMethod]
    public void ErrorView_WithErrorViewModel_Throws_WhenControllerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerExtensions.ErrorView(null!, new ErrorViewModel()));
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_Throws_WhenModelArgumentIsNull()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerExtensions.ErrorView(controller, model: (ErrorViewModel)null!));
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_UsesDefaultErrorViewName_WhenViewNameIsNotSpecified()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var result = ControllerExtensions.ErrorView(controller, new ErrorViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual(ControllerExtensions.DefaultErrorViewName, viewResult.ViewName);
        TypeAssert.IsViewModelType<ErrorViewModel>(result);
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_PresentsNamedView_WhenViewNameIsSpecified()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var result = ControllerExtensions.ErrorView(controller, new ErrorViewModel(), "CustomError");

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("CustomError", viewResult.ViewName);
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_SetsResponseStatusCode_WhenStatusCodeIsProvided()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        ControllerExtensions.ErrorView(controller, new ErrorViewModel { StatusCode = 503 });

        Assert.AreEqual(503, controller.Response.StatusCode);
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_DoesNotChangeResponseStatusCode_WhenStatusCodeIsNotProvided()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };
        controller.Response.StatusCode = 200;

        ControllerExtensions.ErrorView(controller, new ErrorViewModel());

        Assert.AreEqual(200, controller.Response.StatusCode);
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_DoesNotFillRequestId()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext {
                    TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090",
                },
            },
        };

        var result = ControllerExtensions.ErrorView(controller, new ErrorViewModel());

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.IsNull(viewModel.RequestId);
        Assert.IsFalse(viewModel.ShowRequestId);
    }

    [TestMethod]
    public void ErrorView_WithErrorViewModel_PreservesExistingRequestId()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext {
                    TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090",
                },
            },
        };

        var model = new ErrorViewModel {
            RequestId = "existing-request-id",
        };

        var result = ControllerExtensions.ErrorView(controller, model);

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.AreEqual("existing-request-id", viewModel.RequestId);
        Assert.IsTrue(viewModel.ShowRequestId);
    }

    #endregion
}
