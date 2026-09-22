using System.Net;
using Dfe.SignIn.InternalApi.Services.Search;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Dfe.SignIn.InternalApi.UnitTests.Services;

[TestClass]
public class RemoveInviteServiceTests
{
    [TestMethod]
    public async Task DeletesInviteFromSearchIndex()
    {
        // Arrange
        var invitationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString() ==
                        $"https://test/users/inv-{invitationId.ToString().ToUpper()}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(handler.Object) {
            BaseAddress = new Uri("https://test/")
        };

        var logger = new Mock<ILogger<RemoveInviteService>>();

        var sut = new RemoveInviteService(client, logger.Object);

        // Act
        await sut.Handle(userId, invitationId);

        // Assert
        handler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task Handle_LogsWarning_WhenDeleteFails()
    {
        // Arrange
        var invitationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ThrowsAsync(new HttpRequestException());

        var client = new HttpClient(handler.Object);

        var logger = new Mock<ILogger<RemoveInviteService>>();
        var sut = new RemoveInviteService(client, logger.Object);

        // Act
        await sut.Handle(userId, invitationId);

        // Assert
        logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
