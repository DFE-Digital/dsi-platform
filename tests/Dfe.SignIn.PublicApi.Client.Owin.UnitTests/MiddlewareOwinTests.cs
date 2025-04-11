using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.Owin;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.Owin.UnitTests;

[TestClass]
public sealed class MiddlewareOwinTests
{
    #region InvokeAsync(HttpContext)

    [TestMethod]
    public async Task InvokeAsync_InvokesInnerMiddleware()
    {
        var mockMiddleware = new Mock<IHttpMiddleware>();
        mockMiddleware
            .Setup(mock => mock.InvokeAsync(
                It.IsAny<IHttpContext>(),
                It.IsAny<Func<Task>>()
            ))
            .Callback<IHttpContext, Func<Task>>((context, next) => next());

        var adapter = new HttpMiddlewareOwinAdapter(mockMiddleware.Object);

        var mockContext = new Mock<IOwinContext>();
        var mockNext = new Mock<Func<Task>>();

        await adapter.InvokeAsync(mockContext.Object, mockNext.Object);

        mockMiddleware.Verify(mock =>
            mock.InvokeAsync(
                It.Is<IHttpContext>(context => context.Inner == mockContext.Object),
                It.IsAny<Func<Task>>()
            ),
            Times.Once
        );
        mockNext.Verify(next => next(), Times.Once);
    }

    #endregion
}
