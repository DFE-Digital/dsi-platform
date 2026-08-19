using Azure.Core;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.Web.Profile.UnitTests.Services;

[TestClass]
public class GraphApiChangeUserPersonalDetailsTests
{
    private Mock<IPersonalGraphServiceFactory> graphClientFactory = null!;
    private Mock<ILogger<GraphApiChangeUserPersonalDetails>> logger = null!;

    [TestInitialize]
    public void Setup()
    {
        this.graphClientFactory = new Mock<IPersonalGraphServiceFactory>();
        this.logger = new Mock<ILogger<GraphApiChangeUserPersonalDetails>>();
    }

    private GraphApiChangeUserPersonalDetails CreateSut()
    {
        return new GraphApiChangeUserPersonalDetails(
            this.graphClientFactory.Object,
            this.logger.Object);
    }

    [TestMethod]
    public async Task ChangeName_WhenForenameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = this.CreateSut();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            sut.ChangeName(
                Guid.NewGuid(),
                null!,
                "Smith",
                CreateAccessToken()));

        this.graphClientFactory.Verify(
            x => x.GetClient(It.IsAny<AccessToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ChangeName_WhenLastNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = this.CreateSut();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            sut.ChangeName(
                Guid.NewGuid(),
                "John",
                null!,
                CreateAccessToken()));

        this.graphClientFactory.Verify(
            x => x.GetClient(It.IsAny<AccessToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ChangeName_WhenAccessTokenIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = this.CreateSut();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            sut.ChangeName(
                Guid.NewGuid(),
                "John",
                "Smith",
                null));

        this.graphClientFactory.Verify(
            x => x.GetClient(It.IsAny<AccessToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ChangeName_WhenForenameIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = this.CreateSut();

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            sut.ChangeName(
                Guid.NewGuid(),
                string.Empty,
                "Smith",
                CreateAccessToken()));

        // Assert
        Assert.AreEqual("Missing forname", exception.Message);

        this.graphClientFactory.Verify(
            x => x.GetClient(It.IsAny<AccessToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ChangeName_WhenLastNameIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = this.CreateSut();

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            sut.ChangeName(
                Guid.NewGuid(),
                "John",
                string.Empty,
                CreateAccessToken()));

        // Assert
        Assert.AreEqual("Missing lastName", exception.Message);

        this.graphClientFactory.Verify(
            x => x.GetClient(It.IsAny<AccessToken>()),
            Times.Never);
    }

    //todo: review these tests and implement them once the GraphServiceClient is properly mocked and configured.
    //[TestMethod]
    //public async Task ChangeName_WhenValidRequest_GetsGraphClientWithCorrectAccessToken()
    //{
    //    // Arrange
    //    var token = CreateAccessToken("test-token");

    //    var graphClient = CreateGraphClient();

    //    this.graphClientFactory
    //        .Setup(x => x.GetClient(It.IsAny<AccessToken>()))
    //        .Returns(graphClient);

    //    var sut = this.CreateSut();

    //    // Act
    //    await sut.ChangeName(
    //        Guid.NewGuid(),
    //        "John",
    //        "Smith",
    //        token);

    //    // Assert
    //    this.graphClientFactory.Verify(
    //        x => x.GetClient(
    //            It.Is<AccessToken>(accessToken =>
    //                accessToken.Token == token.Token &&
    //                accessToken.ExpiresOn == token.ExpiresOn)),
    //        Times.Once);
    //}

    //[TestMethod]
    //public async Task ChangeName_WhenGraphThrowsODataError_RethrowsException()
    //{
    //    // Arrange
    //    var expectedException = CreateODataError();

    //    var graphClient = CreateGraphClientThatThrows(expectedException);

    //    this.graphClientFactory
    //    .Setup(x => x.GetClient(It.IsAny<AccessToken>()))
    //    .Returns(graphClient);

    //    var sut = this.CreateSut();

    //    // Act
    //    var actualException = await Assert.ThrowsExactlyAsync<ODataError>(() =>
    //        sut.ChangeName(
    //            Guid.NewGuid(),
    //            "John",
    //            "Smith",
    //            CreateAccessToken()));

    //    // Assert
    //    Assert.AreSame(expectedException, actualException);
    //}

    //[TestMethod]
    //public async Task ChangeName_WhenGraphThrowsODataError_LogsError()
    //{
    //    // Arrange
    //    var expectedException = CreateODataError();
    //    var userId = Guid.NewGuid();

    //    var graphClient = CreateGraphClientThatThrows(expectedException);

    //    this.graphClientFactory
    //        .Setup(x => x.GetClient(It.IsAny<AccessToken>()))
    //        .Returns(graphClient);

    //    var sut = this.CreateSut();

    //    // Act
    //    await Assert.ThrowsExactlyAsync<ODataError>(() =>
    //        sut.ChangeName(
    //            userId,
    //            "John",
    //            "Smith",
    //            CreateAccessToken()));

    //    // Assert
    //    this.logger.Verify(
    //        x => x.Log(
    //            LogLevel.Error,
    //            It.IsAny<EventId>(),
    //            It.Is<It.IsAnyType>((state, _) =>
    //                state.ToString()!.Contains(
    //                    $"Failed to patch user userId: {userId}")),
    //            expectedException,
    //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    //        Times.Once);
    //}

    private static GraphAccessToken CreateAccessToken(
        string token = "access-token")
    {
        return new GraphAccessToken {
            Token = token,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
        };
    }

    //private static ODataError CreateODataError()
    //{
    //    return new ODataError {
    //        Error = new MainError {
    //            Code = "TestError",
    //            Message = "Something went wrong."
    //        }
    //    };
    //}

    //private static GraphServiceClient CreateGraphClient()
    //{
    //    // Configure GraphServiceClient with a mocked IRequestAdapter.
    //    throw new NotImplementedException();
    //}

    //private static GraphServiceClient CreateGraphClientThatThrows(
    //    ODataError exception)
    //{
    //    // Configure GraphServiceClient with a mocked IRequestAdapter.
    //    throw new NotImplementedException();
    //}
}
