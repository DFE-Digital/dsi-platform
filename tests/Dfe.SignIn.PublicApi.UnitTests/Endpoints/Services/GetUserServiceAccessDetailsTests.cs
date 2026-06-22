using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Endpoints.Services;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Services;

[TestClass]
public class GetUserServiceAccessDetailsTests
{
    [TestMethod]
    public async Task GetUserServiceAccessDetails_LogsWarningAndReThrows_WhenGuidIsInvalid()
    {
        // Arrange
        const string invalidGuid = "not-a-valid-guid";
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();

        // The endpoint creates a logger using nameof(UserEndpoints)
        loggerFactoryMock
            .Setup(f => f.CreateLogger(nameof(UserEndpoints)))
            .Returns(loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        var interactionMock = new Mock<IInteractionDispatcher>();

        var exception = await Assert.ThrowsExactlyAsync<FormatException>(() =>
            ServiceEndpoints.GetUserServiceAccessDetails(
                Guid.NewGuid().ToString(), // serviceId
                Guid.NewGuid().ToString(), // organisationId
                invalidGuid,               // userId (invalid)
                httpContext,
                loggerFactoryMock.Object,
                interactionMock.Object
            ));

        // Verify the warning was logged with the specific error message pattern
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Error getting user") &&
                    v.ToString()!.Contains(invalidGuid) &&
                    v.ToString()!.Contains("Invalid GUID format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.AreEqual($"Invalid GUID format: '{invalidGuid}'", exception.Message);
    }
}
