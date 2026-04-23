using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using System.Diagnostics;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users;

[TestClass]
public class GetServiceUsersTests
{
    private const string FakeClientId = "test-client-id";
    private static readonly Guid FakeServiceId = Guid.NewGuid();

    private static (AutoMocker, IClientSession, Mock<ILoggerFactory>, DefaultHttpContext) CreateMocks()
    {
        var autoMocker = new AutoMocker();
        var clientSession = autoMocker.GetMock<IClientSession>();
        clientSession.SetupGet(x => x.ClientId).Returns(FakeClientId);

        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-ID"] = "corr-123";
        return (autoMocker, clientSession.Object, loggerFactory, httpContext);
    }

    [TestMethod]
    public async Task Returns400_WhenStatusIsInvalid()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?status=not-a-number");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("Status is not valid. Should be either 0 or 1.", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenToDateIsInvalid()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?to=invalid-date&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("To date is not a valid date.", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenFromDateIsInvalid()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=invalid-date&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("From date is not a valid date.", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenBothDatesInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2099-12-01&to=2099-12-31&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("Date range should not be in the future", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenFromDateInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2099-12-01&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("Date range should not be in the future", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenToDateInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?to=2099-12-31&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("Date range should not be in the future", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenFromDateGreaterThanToDate()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2023-01-10&to=2023-01-01&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("From date greater than to date", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenDateRangeExceedsDuration()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2023-01-01&to=2023-04-02&status=0");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("Only 90 days are allowed between dates", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenPageIsNotANumber()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?page=not-a-number");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("not-a-number is not a valid value for page. Expected a number", badRequest.Value);
    }

    [TestMethod]
    public async Task Returns400_WhenPageSizeIsNotANumber()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?pageSize=not-a-number");

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        Assert.AreEqual("not-a-number is not a valid value for pageSize. Expected a number", badRequest.Value);
    }

    [TestMethod]
    public async Task ReturnsOk_WithValidFilterParameters()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2023-01-01&to=2023-01-05&page=2&pageSize=25&status=0");

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(new GetApplicationByClientIdResponse { 
            Application = new Application { 
                Id = FakeServiceId,
                ClientId = FakeClientId,
                Name = "Test Application",
                IsExternalService = true,
                IsIdOnlyService = false,
                IsHiddenService = false
            } 
        });
        autoMocker.MockResponse<GetServiceUsersRequest>(new GetServiceUsersResponse { 
            Users = [], 
            NumberOfRecords = 0, 
            Page = 2, 
            NumberOfPages = 0 
        });

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>));
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(okResult?.Value);
        Assert.AreEqual("Users between Sun, 01 Jan 2023 00:00:00 GMT and Thu, 05 Jan 2023 00:00:00 GMT", okResult.Value.DateRange);
    }

    [TestMethod]
    public async Task LogsRequest_WithCorrelationIds()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(new GetApplicationByClientIdResponse { 
            Application = new Application { 
                Id = FakeServiceId,
                ClientId = FakeClientId,
                Name = "Test Application",
                IsExternalService = true,
                IsIdOnlyService = false,
                IsHiddenService = false
            } 
        });
        autoMocker.MockResponse<GetServiceUsersRequest>(new GetServiceUsersResponse { 
            Users = [], 
            NumberOfRecords = 0, 
            Page = 1, 
            NumberOfPages = 0 
        });

        await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(FakeClientId) && v.ToString().Contains("corr-123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
