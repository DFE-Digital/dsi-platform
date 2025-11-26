using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Fakes;

public static class FakeResourceContext
{
    public sealed class Flag
    {
        public bool Value { get; set; } = false;
    }

    public static ResourceExecutingContext Create(long? contentLength = null, string method = "POST", long maxRequestBodySize = 1000, int initialResponseStatusCode = StatusCodes.Status200OK)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.ContentLength = contentLength;
        httpContext.Response.StatusCode = initialResponseStatusCode;

        var featureMock = new Mock<IHttpMaxRequestBodySizeFeature>();
        featureMock.Setup(f => f.MaxRequestBodySize).Returns(maxRequestBodySize);
        httpContext.Features.Set(featureMock.Object);

        var actionContext = new ActionContext {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };

        return new ResourceExecutingContext(actionContext, [], []);
    }

    public static ResourceExecutionDelegate CreateNext(out Flag calledFlag)
    {
        var flag = new Flag();
        calledFlag = flag;

        Task<ResourceExecutedContext> Next()
        {
            flag.Value = true;
            return Task.FromResult(new ResourceExecutedContext(
                new ActionContext {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor(),
                },
                []));
        }

        return Next;
    }
}
