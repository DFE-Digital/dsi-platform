using Dfe.SignIn.WebFramework.Controllers;
using Dfe.SignIn.WebFramework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.UnitTests.Controllers;

[TestClass]
public sealed class BaseErrorControllerTests
{
    #region Index()

    [TestMethod]
    public void Index_UsesTraceIdentifierAsRequestId()
    {
        var mocker = new AutoMocker();
        var controller = mocker.CreateInstance<BaseErrorController>();
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                TraceIdentifier = "a492f33c-a859-4098-8c01-b8b2f09a6090"
            }
        };

        var result = controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ErrorViewModel>(result);
        Assert.AreEqual("a492f33c-a859-4098-8c01-b8b2f09a6090", viewModel.RequestId);
    }

    #endregion
}
