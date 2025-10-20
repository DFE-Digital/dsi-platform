using Dfe.SignIn.WebFramework.Mvc.Controllers;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Controllers;

[TestClass]
public sealed class BaseErrorControllerTests
{
    private sealed class FakeErrorController : BaseErrorController
    {
    }

    #region Index(int code)

    [TestMethod]
    public void Index_PresentsNotFoundView_WhenStatusIs404()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };

        var result = controller.Index(404);

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("NotFound", viewResult.ViewName);
        Assert.AreEqual(404, controller.Response.StatusCode);
    }

    [TestMethod]
    public void Index_UsesTraceIdentifierAsRequestId()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext {
                    TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090"
                },
            },
        };

        var result = controller.Index(500);

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.AreEqual("a492f33c-a859-4098-8c01-b8b2f09a6090", viewModel.RequestId);
        Assert.AreEqual(500, controller.Response.StatusCode);
    }

    #endregion
}
