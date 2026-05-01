using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users;

[TestClass]
public class GetServiceUsersTests
{
    private const string FakeClientId = "test-client-id";
    private static readonly Guid FakeServiceId = Guid.NewGuid();

    private static Application CreateTestApplication() => new() {
        Id = FakeServiceId,
        ClientId = FakeClientId,
        Name = "Test Application",
        IsExternalService = true,
        IsIdOnlyService = false,
        IsHiddenService = false
    };

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

    private static void MockApplicationLookup(AutoMocker autoMocker, Application? application = null)
    {
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(new GetApplicationByClientIdResponse {
            Application = application ?? CreateTestApplication()
        });
    }

    private static void MockGetServiceUsersResponse(AutoMocker autoMocker, int page = 1)
    {
        autoMocker.MockResponse<GetServiceUsersRequest>(new GetServiceUsersResponse {
            Users = [],
            NumberOfRecords = 0,
            Page = page,
            NumberOfPages = 0
        });
    }

    [TestMethod]
    public async Task Returns404_WhenApplicationNotFound()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var mockClientSession = autoMocker.GetMock<IClientSession>();
        mockClientSession.SetupGet(x => x.ClientId).Returns("fake-client-id");

        var result = await UserEndpoints.GetServiceUsers(
            mockClientSession.Object,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.NotFound));
    }

    [TestMethod]
    public async Task ReturnsOk_WhenValidRequest()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        MockApplicationLookup(autoMocker);
        var capturedRequest = (GetServiceUsersRequest?)null;
        autoMocker.CaptureRequest<GetServiceUsersRequest>(req => capturedRequest = req, new GetServiceUsersResponse {
            Users = [],
            NumberOfRecords = 0,
            Page = 1,
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
        // Verify request forwarding
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(FakeServiceId, capturedRequest.ApplicationId);
        Assert.AreEqual(1, capturedRequest.PageNumber);
        Assert.AreEqual(25, capturedRequest.PageSize);
    }

    [TestMethod]
    public async Task ExtractParam_IsCaseInsensitive()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?PaGe=2&PaGeSiZe=10");
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker, page: 2);

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>));
    }

    [TestMethod]
    public async Task ReturnsOk_WithValidFilterParameters()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var from = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2023, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var capturedRequest = (GetServiceUsersRequest?)null;

        MockApplicationLookup(autoMocker);
        autoMocker.CaptureRequest<GetServiceUsersRequest>(req => capturedRequest = req, new GetServiceUsersResponse {
            Users = [],
            NumberOfRecords = 0,
            Page = 2,
            NumberOfPages = 0
        });

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            status: 0,
            from: from,
            to: to,
            page: 2,
            pageSize: 25
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>));
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(okResult?.Value);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(FakeServiceId, capturedRequest.ApplicationId);
        Assert.AreEqual(0, capturedRequest.UserStatus);
        Assert.AreEqual(from, capturedRequest.DateFrom);
        Assert.AreEqual(to, capturedRequest.DateTo);
        Assert.AreEqual(2, capturedRequest.PageNumber);
        Assert.AreEqual(25, capturedRequest.PageSize);
    }

    [TestMethod]
    [DataRow(0, 25)]
    [DataRow(1, 0)]
    [DataRow(-1, 25)]
    [DataRow(1, -10)]
    public async Task ReturnsBadRequest_WhenPagingIsInvalid(int page, int pageSize)
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            page: page,
            pageSize: pageSize
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsBadRequest_WhenStatusIsInvalid()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            status: 2
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsBadRequest_WhenFromDateIsGreaterThanToDate()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: new DateTimeOffset(2023, 1, 5, 0, 0, 0, TimeSpan.Zero),
            to: new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsBadRequest_WhenDateRangeExceeds90Days()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            to: new DateTimeOffset(2023, 4, 2, 0, 0, 0, TimeSpan.Zero)
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsBadRequest_WhenDateIsInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: DateTimeOffset.UtcNow.AddDays(1)
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }
    [TestMethod]
    public async Task ReturnsBadRequest_WhenBothDatesAreInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: future,
            to: future.AddDays(1)
        );
        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsBadRequest_WhenOnlyToDateIsInFuture()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            to: future
        );
        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>));
    }

    [TestMethod]
    public async Task ReturnsNotFound_WhenApplicationIsNull()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        // Application is null in response
        autoMocker.MockResponse<GetApplicationByClientIdRequest>(new GetApplicationByClientIdResponse { Application = null });
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );
        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.NotFound));
    }

    [TestMethod]
    public async Task ReturnsProblem_WhenApplicationLookupThrowsUnexpectedException()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        autoMocker.MockThrows<GetApplicationByClientIdRequest>(new Exception("fail"));
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );
        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult));
    }

    [TestMethod]
    public async Task ReturnsOk_SetsWarning_WhenOnlyFromDateProvided()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: from
        );
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(ok);
        Assert.IsTrue(ok.Value?.Warning?.Contains("90 days") ?? false);
    }

    [TestMethod]
    public async Task ReturnsOk_SetsWarning_WhenOnlyToDateProvided()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);
        var to = DateTimeOffset.UtcNow.AddDays(-1);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            to: to
        );
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(ok);
        Assert.IsTrue(ok.Value?.Warning?.Contains("90 days") ?? false);
    }

    [TestMethod]
    public async Task ReturnsOk_NoWarning_WhenBothDatesProvided()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: from,
            to: to
        );
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(ok);
        Assert.IsNull(ok.Value?.Warning);
    }

    [TestMethod]
    public async Task ReturnsOk_SetsDateRange_WhenBothDatesProvided()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext,
            from: from,
            to: to
        );
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(ok);
        Assert.IsNotNull(ok.Value?.DateRange);
    }

    [TestMethod]
    public async Task ReturnsOk_NullDateRange_WhenNoDatesProvided()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);
        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(ok);
        Assert.IsNull(ok.Value?.DateRange);
    }
}
