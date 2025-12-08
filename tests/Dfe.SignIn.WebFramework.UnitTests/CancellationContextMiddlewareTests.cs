using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class CancellationContextMiddlewareTests
{
    [TestMethod]
    public async Task AssignsCancellationTokenFromHttpContext()
    {
        var cancellationContext = new CancellationContext();

        CancellationToken? capturedCancellationToken = null;
        var middleware = new CancellationContextMiddleware(cancellationContext, httpContext => {
            capturedCancellationToken = cancellationContext.CancellationToken;
            return Task.CompletedTask;
        });

        using var cancellationSource = new CancellationTokenSource();
        var httpContext = new DefaultHttpContext {
            RequestAborted = cancellationSource.Token,
        };

        await middleware.InvokeAsync(httpContext);

        Assert.AreEqual(httpContext.RequestAborted, capturedCancellationToken);
        Assert.AreNotEqual(CancellationToken.None, capturedCancellationToken);
    }
}
