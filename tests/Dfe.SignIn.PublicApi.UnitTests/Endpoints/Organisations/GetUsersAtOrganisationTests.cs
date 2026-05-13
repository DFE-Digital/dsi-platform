using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Endpoints.Organisations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Organisations;

[TestClass]
public class GetUsersAtOrganisationTests
{
    private const string FakeClientId = "test-client-id";
    private const string ExternalId = "12345678";

    private static (AutoMocker, IClientSession, Mock<ILoggerFactory>, DefaultHttpContext) CreateMocks()
    {
        var autoMocker = new AutoMocker();

        var clientSession = autoMocker.GetMock<IClientSession>();
        clientSession.SetupGet(x => x.ClientId).Returns(FakeClientId);

        var loggerFactory = new Mock<ILoggerFactory>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["x-correlation-id"] = "corr-123";

        return (autoMocker, clientSession.Object, loggerFactory, httpContext);
    }

    [TestMethod]
    public async Task ReturnsOk_WhenUsersExist()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var users = new[]
        {
            new UserAtOrganisationRaw
            (
                Guid.NewGuid(),
                "user1@test.com",
                "John",
                "Doe",
                0,
                "Admin"
            )
        };

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(
            new GetUsersAtOrganisationResponseRaw {
                Users = users,
                IsUkprn = true
            });

        var response = await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        Ok<GetUsersAtOrganisationResponse> ok = response.Result as Ok<GetUsersAtOrganisationResponse>;
        Assert.IsNotNull(ok);
        Assert.AreEqual(1, ok.Value!.Users.Count);
        Assert.AreEqual("user1@test.com", ok.Value.Users.First().Email);
    }

    [TestMethod]
    public async Task ReturnsNotFound_WhenNoUsers()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(
            new GetUsersAtOrganisationResponseRaw {
                Users = [],
                IsUkprn = true
            });

        Results<Ok<GetUsersAtOrganisationResponse>, NotFound, InternalServerError<ProblemDetails>> response = await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        Assert.IsInstanceOfType(response.Result, typeof(NotFound));
    }

    [TestMethod]
    public async Task ReturnsNotFound_WhenModelIsNull()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(null);

        var response = await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        Assert.IsInstanceOfType(response.Result, typeof(NotFound));
    }

    [TestMethod]
    public async Task GroupsUsers_BySub_AndDistinctRoles()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var users = new[]
        {
            new UserAtOrganisationRaw(Guid.Empty, "user@test.com", "John", "Doe", 1, "Admin"),
            new UserAtOrganisationRaw(Guid.Empty, "user@test.com", "John", "Doe", 1, "Admin"),
            new UserAtOrganisationRaw(Guid.Empty, "user@test.com", "John", "Doe", 1, "User")
        };

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(
            new GetUsersAtOrganisationResponseRaw {
                Users = users,
                IsUkprn = true
            });

        var response = await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        Ok<GetUsersAtOrganisationResponse> ok = response.Result as Ok<GetUsersAtOrganisationResponse>;
        Assert.IsNotNull(ok);

        var user = ok.Value!.Users.Single();

        Assert.AreEqual(2, user.Roles.Count());
        CollectionAssert.AreEquivalent(new[] { "Admin", "User" }, user.Roles.ToArray());
    }

    [TestMethod]
    public async Task FiltersOutEmptyRoles()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var users = new[]
        {
            new UserAtOrganisationRaw(Guid.Empty, "user@test.com", "John", "Doe", 1, ""),
            new UserAtOrganisationRaw(Guid.Empty, "user@test.com", "John", "Doe", 1, "Admin"),
        };

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(
            new GetUsersAtOrganisationResponseRaw {
                Users = users,
                IsUkprn = true
            });

        var response = await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        Ok<GetUsersAtOrganisationResponse> ok = response.Result as Ok<GetUsersAtOrganisationResponse>;
        Assert.IsNotNull(ok);

        var roles = ok.Value!.Users.First().Roles;

        Assert.AreEqual(1, roles.Count());
        Assert.AreEqual("Admin", roles.First());
    }

    [TestMethod]
    public async Task LogsRequest_WithCorrelationIds()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(logger.Object);

        var users = new[]
        {
            new UserAtOrganisationRaw(Guid.NewGuid(), "user@test.com", "John", "Doe", 1, "Admin")
        };

        autoMocker.MockResponse<GetUsersAtOrganisationRequestRaw>(
            new GetUsersAtOrganisationResponseRaw {
                Users = users,
                IsUkprn = true
            });

        await OrganisationEndpoints.GetUsersAtOrganisation(
            ExternalId,
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext);

        var logInvocation = logger.Invocations.FirstOrDefault(i => i.Method.Name == "Log");

        Assert.IsNotNull(logInvocation);

        var message = logInvocation.Arguments[2]?.ToString();
        Assert.IsNotNull(message);
        Assert.Contains(FakeClientId, message);
        Assert.Contains("corr-123", message);
    }
}
