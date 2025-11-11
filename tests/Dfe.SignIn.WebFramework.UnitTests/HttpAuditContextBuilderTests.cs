using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class HttpAuditContextBuilderTests
{
    private static HttpAuditContextBuilder CreateBuilder(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptions<AuditOptions>>()
            .Setup(x => x.Value)
            .Returns(new AuditOptions {
                ApplicationName = "FakeApplication",
                EnvironmentName = "local",
            });

        return autoMocker.CreateInstance<HttpAuditContextBuilder>();
    }

    private static void SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(x => x.Headers).Returns(new HeaderDictionary());

        var mockConnectionInfo = autoMocker.GetMock<ConnectionInfo>();
        mockConnectionInfo.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));

        var mockContext = autoMocker.GetMock<HttpContext>();
        mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
        mockContext.Setup(x => x.User).Returns(new ClaimsPrincipal());
        mockContext.Setup(x => x.Connection).Returns(mockConnectionInfo.Object);

        autoMocker.GetMock<IHttpContextAccessor>()
            .Setup(x => x.HttpContext)
            .Returns(mockContext.Object);
    }

    #region BuildAuditContext()

    [TestMethod]
    public void BuildAuditContext_WhenNoHttpContext()
    {
        var builder = CreateBuilder(new AutoMocker());

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("FakeApplication", auditContext.SourceApplication);
        Assert.IsNull(auditContext.SourceUserId);
    }

    [TestMethod]
    public void BuildAuditContext_WhenNothingFromHttpContext()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("FakeApplication", auditContext.SourceApplication);
        Assert.IsNull(auditContext.SourceUserId);
    }

    [TestMethod]
    public void BuildAuditContext_WhenSourceAppHeaderPresent()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(x => x.Headers).Returns(new HeaderDictionary {
            [AuditHeaderNames.SourceApplicationName] = "FakeSourceApplication",
        });

        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("FakeSourceApplication", auditContext.SourceApplication);
    }

    [TestMethod]
    public void BuildAuditContext_WhenSourceIpHeaderPresent()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(x => x.Headers).Returns(new HeaderDictionary {
            [AuditHeaderNames.SourceIpAddress] = "127.0.0.1",
        });

        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("127.0.0.1", auditContext.SourceIpAddress);
    }

    [TestMethod]
    public void BuildAuditContext_WhenSourceUserHeaderPresent()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(x => x.Headers).Returns(new HeaderDictionary {
            [AuditHeaderNames.SourceUserId] = "f584969f-e05d-46d7-8a05-585318730d09",
        });

        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("f584969f-e05d-46d7-8a05-585318730d09", auditContext.SourceUserId.ToString());
    }

    [TestMethod]
    public void BuildAuditContext_WhenUserHasDsiUserIdClaim()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);

        var mockContext = autoMocker.GetMock<HttpContext>();
        mockContext.Setup(x => x.User).Returns(new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim("dsi_user_id", "64b04c9d-0eb8-417d-b29d-8fc1d33a1ecc"),
                new Claim(ClaimTypes.NameIdentifier, "1d2db0a6-69c1-43bb-a2fb-e0baf7fa12f1"),
            ], "fake")
        ]));

        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("64b04c9d-0eb8-417d-b29d-8fc1d33a1ecc", auditContext.SourceUserId.ToString());
    }

    [TestMethod]
    public void BuildAuditContext_WhenUserHasLegacyIdClaim()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);

        var mockContext = autoMocker.GetMock<HttpContext>();
        mockContext.Setup(x => x.User).Returns(new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "1d2db0a6-69c1-43bb-a2fb-e0baf7fa12f1"),
            ], "fake")
        ]));

        var builder = CreateBuilder(autoMocker);

        using var activity = new Activity("TestActivity").Start();
        var auditContext = builder.BuildAuditContext();

        Assert.AreEqual(Activity.Current?.TraceId.ToString(), auditContext.TraceId);
        Assert.AreEqual("1d2db0a6-69c1-43bb-a2fb-e0baf7fa12f1", auditContext.SourceUserId.ToString());
    }

    #endregion
}
