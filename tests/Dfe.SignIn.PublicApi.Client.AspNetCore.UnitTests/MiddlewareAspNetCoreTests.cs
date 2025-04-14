using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class MiddlewareAspNetCoreTests
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

        var mockNext = new Mock<RequestDelegate>();
        var adapter = new HttpMiddlewareAspNetCoreAdapter(mockMiddleware.Object, mockNext.Object);

        var mockContext = new Mock<HttpContext>();

        await adapter.InvokeAsync(mockContext.Object);

        mockMiddleware.Verify(mock =>
            mock.InvokeAsync(
                It.Is<IHttpContext>(context => context.Inner == mockContext.Object),
                It.IsAny<Func<Task>>()
            ),
            Times.Once
        );
        mockNext.Verify(next =>
            next(
                It.Is<HttpContext>(context => context == mockContext.Object)
            ),
            Times.Once
        );
    }

    #endregion
}
