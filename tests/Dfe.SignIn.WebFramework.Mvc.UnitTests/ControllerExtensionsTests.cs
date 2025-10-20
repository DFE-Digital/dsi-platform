using Dfe.SignIn.WebFramework.Mvc.Controllers;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class ControllerExtensionsTests
{
    private sealed class FakeErrorController : BaseErrorController { }

    #region ErrorView(Controller, string?, ErrorViewModel?)

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

        TypeAssert.IsViewModelType<ErrorViewModel>(result);
    }

    [TestMethod]
    public void ErrorView_PresentsGivenViewModel_WhenModelArgumentIsSpecified()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var model = new ErrorViewModel();

        var result = ControllerExtensions.ErrorView(controller, model: model);

        var actualModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
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

        var result = controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.AreEqual("a492f33c-a859-4098-8c01-b8b2f09a6090", viewModel.RequestId);
    }

    #endregion
}
