using Dfe.SignIn.WebFramework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Filters;

[TestClass]
public sealed class LogAntiforgeryFailureFilterTests
{
    private readonly Mock<ILogger<LogAntiforgeryFailureFilter>> loggerMock = new();
    private readonly LogAntiforgeryFailureFilter filter;

    public LogAntiforgeryFailureFilterTests()
    {
        this.filter = new LogAntiforgeryFailureFilter(this.loggerMock.Object);
    }

    private static ResultExecutedContext CreateContext(IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        return new ResultExecutedContext(actionContext, Array.Empty<IFilterMetadata>(), result, controller: new object()) {
            Canceled = false
        };
    }

    [TestMethod]
    public void AntiforgeryFailed_LogsError()
    {
        var context = CreateContext(new AntiforgeryValidationFailedResult());
        this.filter.OnResultExecuted(context);

        this.loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Antiforgery validation failed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public void NonAntiforgeryResult_DoesNotLog()
    {
        var context = CreateContext(new OkResult());
        this.filter.OnResultExecuted(context);
        this.loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public void OnResultExecuting_IsNoOp()
    {
        var context = new ResultExecutingContext(
            new ActionContext(
                new DefaultHttpContext(),
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            ),
            Array.Empty<IFilterMetadata>(),
            new OkResult(),
            controller: new object()
        );

        this.filter.OnResultExecuting(context);
        Assert.IsInstanceOfType(context.Result, typeof(OkResult));
    }
}
