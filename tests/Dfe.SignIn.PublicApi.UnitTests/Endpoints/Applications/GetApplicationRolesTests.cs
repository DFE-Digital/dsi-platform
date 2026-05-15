using Dfe.SignIn.Base.Framework;
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
    public async Task ReturnsOkWithRoles_WhenRequesterIsParent()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        // Simulate requester is parent of the application
        var parentClientId = "parent-client-id";
        var parentId = new Guid("660e8400-e29b-41d4-a716-446655440000");
        var appWithParent = FakeApplication with { ClientId = "child-client-id", ParentClientId = parentClientId, ParentId = parentId };
        clientSession = autoMocker.GetMock<IClientSession>().Object;
        autoMocker.GetMock<IClientSession>().SetupGet(x => x.ClientId).Returns(parentClientId);

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = appWithParent }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(FakeRolesResponse);

        var result = await ApplicationEndpoints.GetApplicationRoles(
            "child-client-id",
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType<Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>>(result);
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(1, okResult.Value!.Count());
    }

    [TestMethod]
    public async Task ReturnsOkWithRoles_WhenRequesterIsSelf()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        // Simulate requester is the application itself
        autoMocker.GetMock<IClientSession>().SetupGet(x => x.ClientId).Returns(FakeClientId);
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

        Assert.IsInstanceOfType<Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>>(result);
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(1, okResult.Value!.Count());
    }

    [TestMethod]
    public async Task ReturnsEmptyArray_WhenNoRoles()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(new GetApplicationRolesResponse { Roles = [] });

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType<Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>>(result);
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(0, okResult.Value!.Count());
    }

    [TestMethod]
    public async Task ReturnsOnlyNameCodeStatusProperties_AsStrings()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var roles = new[]
        {
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role One", Code = "Role1", NumericId = 1, Status = ApplicationRoleStatus.Active },
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role Two", Code = "Role2", NumericId = 2, Status = ApplicationRoleStatus.Inactive }
        };
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(new GetApplicationRolesResponse { Roles = roles });

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        foreach (var role in okResult.Value!) {
            var props = role.GetType().GetProperties().Select(p => p.Name).OrderBy(x => x).ToArray();
            CollectionAssert.AreEqual(new[] { "Code", "Name", "Status" }, props);
            Assert.IsInstanceOfType<string>(role.Name);
            Assert.IsInstanceOfType<string>(role.Code);
            Assert.IsInstanceOfType<string>(role.Status);
        }
    }

    [TestMethod]
    public async Task ReturnsCorrectNameAndCode_ForMultipleRoles()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var roles = new[]
        {
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role One", Code = "Role1", NumericId = 1, Status = ApplicationRoleStatus.Active },
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role Two", Code = "Role2", NumericId = 2, Status = ApplicationRoleStatus.Inactive }
        };
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(new GetApplicationRolesResponse { Roles = roles });

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        var resultList = okResult.Value!.ToList();
        Assert.AreEqual("Role One", resultList[0].Name);
        Assert.AreEqual("Role1", resultList[0].Code);
        Assert.AreEqual("Role Two", resultList[1].Name);
        Assert.AreEqual("Role2", resultList[1].Code);
    }

    [TestMethod]
    public async Task ReturnsStatusAsInactive_IfStatusIdIsZero()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var roles = new[]
        {
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role One", Code = "Role1", NumericId = 1, Status = 0 }
        };
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(new GetApplicationRolesResponse { Roles = roles });

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("Inactive", okResult.Value!.First().Status);
    }

    [TestMethod]
    public async Task ReturnsStatusAsInactive_IfStatusPropertyMissing()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        // Simulate missing status by using default value (0)
        var roles = new[]
        {
            new ApplicationRole { Id = Guid.NewGuid(), Name = "Role One", Code = "Role1", NumericId = 1, Status = 0  }
        };
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetApplicationRolesRequest>(new GetApplicationRolesResponse { Roles = roles });

        var result = await ApplicationEndpoints.GetApplicationRoles(
            FakeClientId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("Inactive", okResult.Value!.First().Status);
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

        Assert.IsInstanceOfType<IResult>(result);
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<IEnumerable<ApplicationRoleDto>>;
        Assert.IsNotNull(okResult);
        var roles = okResult.Value!.ToList();
        Assert.HasCount(1, roles);
        Assert.AreEqual("DSI Child One", roles[0].Name);
        Assert.AreEqual("DSI_Child_One", roles[0].Code);
        Assert.AreEqual("Active", roles[0].Status);
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

        Assert.IsInstanceOfType<Microsoft.AspNetCore.Http.HttpResults.ForbidHttpResult>(result);
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
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
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
        Assert.Contains(FakeClientId, logMessage);
        Assert.Contains("corr-123", logMessage);
    }
}
