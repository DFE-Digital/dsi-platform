using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.Owin.UnitTests;

[TestClass]
public sealed class HttpContextOwinTests
{
    private static Mock<IOwinContext> SetupMockOwinContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IOwinContext>();

        var mockAuthManager = autoMocker.GetMock<IAuthenticationManager>();
        mockAuthManager.Setup(mock => mock.User).Returns(new ClaimsPrincipal());
        mockContext.Setup(mock => mock.Authentication).Returns(mockAuthManager.Object);

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockContext.Setup(mock => mock.Request).Returns(mockRequest.Object);

        var mockResponse = autoMocker.GetMock<IOwinResponse>();
        mockContext.Setup(mock => mock.Response).Returns(mockResponse.Object);

        return mockContext;
    }

    #region IHttpContext

    [TestMethod]
    public void IHttpContext_Inner_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        Assert.AreSame(mockContext.Object, adapter.Inner);
    }

    [TestMethod]
    public void IHttpContext_Request_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        Assert.AreSame(adapter, adapter.Request);
    }

    [TestMethod]
    public void IHttpContext_Response_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        Assert.AreSame(adapter, adapter.Request);
    }

    [TestMethod]
    public void IHttpContext_User_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        Assert.IsNotNull(adapter.User);
        Assert.AreSame(mockContext.Object.Authentication.User, adapter.User);
    }

    [TestMethod]
    public async Task IHttpContext_SignInAsync_InvokesInnerSignInAsync()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var newPrincipal = new ClaimsPrincipal([
            new ClaimsIdentity("Cookies"),
            new ClaimsIdentity(PublicApiConstants.AuthenticationType),
        ]);

        await adapter.SignInAsync(newPrincipal);

        autoMocker.Verify<IAuthenticationManager>(authManager =>
            authManager.SignOut(
                It.Is<string[]>(authenticationTypes =>
                    authenticationTypes.Length == 1 &&
                    authenticationTypes[0] == PublicApiConstants.AuthenticationType
                )
            ),
            Times.Once
        );
        autoMocker.Verify<IAuthenticationManager>(authManager =>
            authManager.SignIn(
                It.Is<AuthenticationProperties>(properties =>
                    properties.IsPersistent
                ),
                It.Is<ClaimsIdentity[]>(identities =>
                    identities.Length == 2 &&
                    identities[0].AuthenticationType == "Cookies" &&
                    identities[1].AuthenticationType == PublicApiConstants.AuthenticationType
                )
            ),
            Times.Once
        );
    }

    #endregion

    #region IHttpRequest

    [TestMethod]
    public void IHttpRequest_Method_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock => mock.Method).Returns("PATCH");

        Assert.AreSame("PATCH", adapter.Request.Method);
    }

    [TestMethod]
    public void IHttpRequest_Scheme_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock => mock.Scheme).Returns("https");

        Assert.AreSame("https", adapter.Request.Scheme);
    }

    [TestMethod]
    public void IHttpRequest_Host_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock => mock.Host).Returns(new HostString("localhost:3000"));

        Assert.AreSame("localhost:3000", adapter.Request.Host);
    }

    [TestMethod]
    public void IHttpRequest_PathBase_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock => mock.PathBase).Returns(new PathString("/app"));

        Assert.AreSame("/app", adapter.Request.PathBase);
    }

    [TestMethod]
    public void IHttpRequest_Path_HasExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock => mock.Path).Returns(new PathString("/entra/callback"));

        Assert.AreSame("/entra/callback", adapter.Request.Path);
    }

    [TestMethod]
    public async Task IHttpRequest_ReadFormAsync_ReturnsExpectedValue()
    {
        var autoMocker = new AutoMocker();
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        var mockRequest = autoMocker.GetMock<IOwinRequest>();
        mockRequest.Setup(mock =>
            mock.ReadFormAsync()
        ).ReturnsAsync(
            new FormCollection(new Dictionary<string, string[]> {
                { "FirstKey", new string[] { "first" } },
                { "SecondKey", new string[] { "second" } },
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
        SetupMockOwinContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextOwinAdapter>();

        adapter.Response.Redirect("/new-location");

        autoMocker.Verify<IOwinResponse>(mock =>
            mock.Redirect(
                It.Is<string>(location => location == "/new-location")
            ),
            Times.Once
        );
    }

    #endregion
}
