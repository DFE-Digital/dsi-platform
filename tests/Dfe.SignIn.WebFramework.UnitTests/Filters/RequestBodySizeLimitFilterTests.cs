
using Dfe.SignIn.WebFramework.Filters;
using Dfe.SignIn.WebFramework.UnitTests.Fakes;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.WebFramework.UnitTests.Filters;

[TestClass]
public sealed class RequestBodySizeLimitFilterTests
{
    private readonly RequestBodySizeLimitFilter filter = new();

    [TestMethod]
    [DataRow("POST", 500, 1000)]
    [DataRow("PUT", 800, 1000)]
    [DataRow("PATCH", 100, 500)]
    [DataRow("DELETE", 300, 1000)]
    public async Task RequestsUnderLimit_CallsNext(string method, long contentLength, long maxRequestBodySize)
    {
        var context = FakeResourceContext.Create(contentLength: contentLength, method: method, maxRequestBodySize: maxRequestBodySize);
        var next = FakeResourceContext.CreateNext(out var nextCalled);

        await this.filter.OnResourceExecutionAsync(context, next);

        Assert.IsTrue(nextCalled.Value);
        Assert.AreEqual(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    [DataRow("POST", 1500, 1000)]
    [DataRow("PUT", 2000, 1500)]
    [DataRow("PATCH", 600, 500)]
    [DataRow("DELETE", 1200, 1000)]
    public async Task RequestsOverLimit_Sets413AndSkipsNext(string method, long contentLength, long maxRequestBodySize)
    {
        var context = FakeResourceContext.Create(contentLength: contentLength, method: method, maxRequestBodySize: maxRequestBodySize);
        var next = FakeResourceContext.CreateNext(out var nextCalled);

        await this.filter.OnResourceExecutionAsync(context, next);

        Assert.IsFalse(nextCalled.Value);
        Assert.AreEqual(StatusCodes.Status413RequestEntityTooLarge, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    [DataRow("GET")]
    [DataRow("HEAD")]
    public async Task NonBodyMethods_CallsNext(string method)
    {
        var context = FakeResourceContext.Create(contentLength: 5000, method: method, maxRequestBodySize: 1000);
        var next = FakeResourceContext.CreateNext(out var nextCalled);

        await this.filter.OnResourceExecutionAsync(context, next);

        Assert.IsTrue(nextCalled.Value);
        Assert.AreEqual(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task Already413Response_CallsNextWithoutOverride()
    {
        var context = FakeResourceContext.Create(contentLength: 5000, method: "POST", maxRequestBodySize: 1000);
        context.HttpContext.Response.StatusCode = StatusCodes.Status413RequestEntityTooLarge;

        var next = FakeResourceContext.CreateNext(out var nextCalled);

        await this.filter.OnResourceExecutionAsync(context, next);

        Assert.IsTrue(nextCalled.Value);
        Assert.AreEqual(StatusCodes.Status413RequestEntityTooLarge, context.HttpContext.Response.StatusCode);
    }
}
