using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class HttpContextAspNetCoreTests
{
    private static Mock<HttpContext> SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        mockContext.Setup(mock => mock.User).Returns(new ClaimsPrincipal());

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockContext.Setup(mock => mock.Request).Returns(mockRequest.Object);

        var mockResponse = autoMocker.GetMock<HttpResponse>();
        mockContext.Setup(mock => mock.Response).Returns(mockResponse.Object);

        return mockContext;
    }

    #region IHttpContext

    [TestMethod]
    public void IHttpContext_Inner_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        Assert.AreSame(mockContext.Object, adapter.Inner);
    }

    [TestMethod]
    public void IHttpContext_Request_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        Assert.AreSame(adapter, adapter.Request);
    }

    [TestMethod]
    public void IHttpContext_Response_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        Assert.AreSame(adapter, adapter.Request);
    }

    [TestMethod]
    public void IHttpContext_User_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        Assert.IsNotNull(adapter.User);
        Assert.AreSame(mockContext.Object.User, adapter.User);
    }

    [TestMethod]
    public async Task IHttpContext_SignInAsync_InvokesInnerSignInAsync()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockAuthService = new Mock<IAuthenticationService>();
        var services = new ServiceCollection();
        services.AddSingleton(mockAuthService.Object);
        mockContext.Setup(mock => mock.RequestServices).Returns(services.BuildServiceProvider());

        var newPrincipal = new ClaimsPrincipal();

        await adapter.SignInAsync(newPrincipal);

        mockAuthService.Verify(authService =>
            authService.SignInAsync(
                It.Is<HttpContext>(context => context == mockContext.Object),
                It.Is<string>(scheme => scheme == null),
                It.Is<ClaimsPrincipal>(principal => principal == newPrincipal),
                It.IsAny<AuthenticationProperties>()
            )
        );
    }

    #endregion

    #region IHttpRequest

    [TestMethod]
    public void IHttpRequest_Method_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("PATCH");

        Assert.AreSame("PATCH", adapter.Request.Method);
    }

    [TestMethod]
    public void IHttpRequest_Scheme_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Scheme).Returns("https");

        Assert.AreSame("https", adapter.Request.Scheme);
    }

    [TestMethod]
    public void IHttpRequest_Host_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Host).Returns(new HostString("localhost:3000"));

        Assert.AreSame("localhost:3000", adapter.Request.Host);
    }

    [TestMethod]
    public void IHttpRequest_PathBase_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.PathBase).Returns(new PathString("/app"));

        Assert.AreSame("/app", adapter.Request.PathBase);
    }

    [TestMethod]
    public void IHttpRequest_Path_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Path).Returns(new PathString("/entra/callback"));

        Assert.AreSame("/entra/callback", adapter.Request.Path);
    }

    [TestMethod]
    public async Task IHttpRequest_ReadFormAsync_ReturnsExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock =>
            mock.ReadFormAsync(
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(
            new FormCollection(new() {
                { "FirstKey", "first" },
                { "SecondKey", "second" },
            })
        );

        var form = await adapter.Request.ReadFormAsync();

        Assert.AreEqual(2, form.Count);
        Assert.AreEqual("first", form["FirstKey"]);
        Assert.AreEqual("second", form["SecondKey"]);
    }

    #endregion

    #region IHttpResponse

    [TestMethod]
    public void IHttpResponse_Redirect_InvokesInnerRedirect()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        adapter.Response.Redirect("/new-location");

        autoMocker.Verify<HttpResponse>(mock =>
            mock.Redirect(
                It.Is<string>(location => location == "/new-location")
            ),
            Times.Once
        );
    }

    #endregion
}
