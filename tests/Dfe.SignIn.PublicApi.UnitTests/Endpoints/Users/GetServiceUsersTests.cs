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
    public async Task Returns404_WhenApplicationNotFound()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        //MockApplicationLookup(autoMocker, null);

        //autoMocker.MockResponse<GetApplicationByClientIdRequest>(new GetApplicationByClientIdResponse {
        //    Application = null
        //});

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
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
        // No query params: should use defaults page=1, pageSize=25
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
        Assert.AreEqual(FakeServiceId, capturedRequest!.ApplicationId);
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
    public async Task LogsRequest_WithCorrelationIds()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        var logger = new Mock<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker);

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

    [TestMethod]
    public async Task ReturnsOk_WithValidFilterParameters()
    {
        var (autoMocker, clientSession, loggerFactory, httpContext) = CreateMocks();
        httpContext.Request.QueryString = new QueryString("?from=2023-01-01&to=2023-01-05&page=2&pageSize=25&status=0");

        MockApplicationLookup(autoMocker);
        MockGetServiceUsersResponse(autoMocker, page: 2);

        var result = await UserEndpoints.GetServiceUsers(
            clientSession,
            autoMocker.Get<IInteractionDispatcher>(),
            loggerFactory.Object,
            httpContext
        );

        Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>));
        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetServiceUsersResponse>;
        Assert.IsNotNull(okResult?.Value);
    }
}
