using Dfe.SignIn.WebFramework.Mvc.Controllers;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    public void Index_RedirectToRoot_WhenGetRequestStatusIs405()
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext(),
            },
        };
        controller.Request.Method = HttpMethods.Get;

        var result = controller.Index(405);

        var redirectResult = TypeAssert.IsType<RedirectResult>(result);
        Assert.AreEqual("/", redirectResult.Url);
    }

    [TestMethod]
    [DataRow(405)]
    [DataRow(500)]
    public void Index_UsesTraceIdentifierAsRequestId(int statusCode)
    {
        var controller = new FakeErrorController {
            ControllerContext = new() {
                HttpContext = new DefaultHttpContext {
                    TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090"
                },
            },
        };

        var result = controller.Index(statusCode);

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.AreEqual("a492f33c-a859-4098-8c01-b8b2f09a6090", viewModel.RequestId);
        Assert.AreEqual(statusCode, controller.Response.StatusCode);
    }

    #endregion
}
