using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.Applications;
using Dfe.SignIn.PublicApi.Endpoints.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Applications;

[TestClass]
public class GetApplicationRolesTests
{
    private const string FakeClientId = "test-client-id";
    private static readonly Guid FakeApplicationId = new("550e8400-e29b-41d4-a716-446655440000");

    private static readonly Application FakeApplication = new() {
        Id = FakeApplicationId,
        ClientId = FakeClientId,
        Name = "Test Application",
        Description = "A test application",
        IsExternalService = false,
        IsHiddenService = false,
        IsIdOnlyService = false
    };

    private static readonly ApplicationRole FakeCoreRole = new() {
        Id = new Guid("a5a8e401-e29b-41d4-a716-446655440001"),
        Code = "DSI_Child_One",
        Name = "DSI Child One",
        NumericId = 1,
        Status = ApplicationRoleStatus.Active,
        Parent = null
    };

    private static readonly GetApplicationRolesResponse FakeRolesResponse = new() {
        Roles = [FakeCoreRole]
    };

    private static (AutoMocker, IClientSession, Mock<ILoggerFactory>, DefaultHttpContext) CreateMocks()
    {
        var autoMocker = new AutoMocker();
        var clientSession = autoMocker.GetMock<IClientSession>();
        clientSession.SetupGet(x => x.ClientId).Returns(FakeClientId);

        var loggerFactory = new Mock<ILoggerFactory>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-ID"] = "corr-123";
        return (autoMocker, clientSession.Object, loggerFactory, httpContext);
    }

    [TestMethod]
    public async Task ReturnsOkWithRoles_WhenAuthorized()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(FakeRolesResponse);

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(IResult));
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        var roles = okResult.Value.ToList();
        Assert.AreEqual(1, roles.Count);
        Assert.AreEqual("DSI Child One", roles[0].Name);
        Assert.AreEqual("DSI_Child_One", roles[0].Code);
        Assert.AreEqual("Active", roles[0].Status);
    }

    [TestMethod]
    public async Task ReturnsNotFound_WhenApplicationIsNull()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = null }
        );

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.NotFound));
    }

    [TestMethod]
    public async Task ReturnsForbid_WhenClientIdDoesNotMatch()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var otherApp = FakeApplication with { ClientId = "other-client-id" };
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = otherApp }
        );

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.ForbidHttpResult));
    }

    [TestMethod]
    public async Task LogsRequest_WithCorrelationIds()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(FakeRolesResponse);

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        // Assert log was called and contains expected values
        var logInvocation = logger.Invocations.FirstOrDefault(inv => inv.Method.Name == "Log");
        Assert.IsNotNull(logInvocation, "Log was not called");
        var logMessage = logInvocation.Arguments[2]?.ToString();
        Assert.IsNotNull(logMessage);
        Assert.IsTrue(logMessage.Contains(FakeClientId));
        Assert.IsTrue(logMessage.Contains("corr-123"));
    }
}
