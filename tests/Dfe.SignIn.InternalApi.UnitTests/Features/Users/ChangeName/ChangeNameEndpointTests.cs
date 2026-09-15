using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeName;
using Dfe.SignIn.InternalApi.Features.Users.ChangeName;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.InternalApi.UnitTests.Features.Users.ChangeName;

[TestClass]
public sealed class ChangeNameEndpointTests
{
    private DbDirectoriesContext dbContext = null!;
    private Mock<IAuditWriter> auditWriterMock = null!;
    private Mock<IEventPublisher> eventPublisherMock = null!;
    private Mock<IEntraChangeNameService> entraChangeNameServiceMock = null!;
    private Mock<ILogger<ChangeNameEndpoint>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new DbDirectoriesContext(options);
        this.auditWriterMock = new Mock<IAuditWriter>();
        this.eventPublisherMock = new Mock<IEventPublisher>();
        this.entraChangeNameServiceMock = new Mock<IEntraChangeNameService>();
        this.loggerMock = new Mock<ILogger<ChangeNameEndpoint>>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.dbContext.Dispose();
    }

    private ChangeNameEndpoint CreateEndpoint()
    {
        return new ChangeNameEndpoint(
            this.dbContext,
            this.auditWriterMock.Object,
            this.eventPublisherMock.Object,
            this.entraChangeNameServiceMock.Object,
            this.loggerMock.Object);
    }

    private static UserEntity CreateTestUser(Guid userId, string firstName = "John", string lastName = "Doe", bool isEntra = false, Guid? entraOid = null)
    {
        return new UserEntity {
            Sub = userId,
            Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
            FirstName = firstName,
            LastName = lastName,
            Password = "hashed-password",
            Salt = "salt-value",
            Status = 1,
            IsEntra = isEntra,
            EntraOid = entraOid
        };
    }

    [TestMethod]
    public async Task HandleAsync_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var endpoint = this.CreateEndpoint();
        var request = new ChangeNameRequest { FirstName = "Jane", LastName = "Smith" };

        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<NotFound>(result);
        this.auditWriterMock.Verify(x => x.Log(It.IsAny<WriteToAuditRequest>()), Times.Never);
        this.eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<UserUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_WhenNameIsUnchanged_ReturnsOk_AndDoesNotUpdateDbOrPublishEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId, "John", "Doe");
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        var endpoint = this.CreateEndpoint();
        var request = new ChangeNameRequest { FirstName = "John   ", LastName = "  Doe" };

        // Act
        var result = await endpoint.HandleAsync(userId, request, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<Ok>(result);
        this.auditWriterMock.Verify(x => x.Log(It.IsAny<WriteToAuditRequest>()), Times.Never);
        this.eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<UserUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        this.entraChangeNameServiceMock.Verify(x => x.ChangeNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_WhenNonEntraUser_UpdatesDb_WritesAudit_AndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId, "John", "Doe", isEntra: false);
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        var endpoint = this.CreateEndpoint();
        var request = new ChangeNameRequest { FirstName = "Jane", LastName = "Smith" };

        // Act
        var result = await endpoint.HandleAsync(userId, request, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<Ok>(result);

        var updatedUser = await this.dbContext.Users.SingleAsync(x => x.Sub == userId);
        Assert.AreEqual("Jane", updatedUser.FirstName);
        Assert.AreEqual("Smith", updatedUser.LastName);

        this.auditWriterMock.Verify(x => x.Log(It.Is<WriteToAuditRequest>(a =>
            a.UserId == userId &&
            a.EventCategory == AuditEventCategoryNames.ChangeName &&
            a.Message == "Successfully changed users name to Jane Smith"
        )), Times.Once);

        this.eventPublisherMock.Verify(x => x.PublishAsync(It.Is<UserUpdatedEvent>(e =>
            e.UserId == userId &&
            e.FirstName == "Jane" &&
            e.LastName == "Smith"
        ), It.IsAny<CancellationToken>()), Times.Once);

        this.entraChangeNameServiceMock.Verify(x => x.ChangeNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_WhenEntraUserSucceeds_UpdatesEntra_AndCompletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraOid = Guid.NewGuid();
        var user = CreateTestUser(userId, "John", "Doe", isEntra: true, entraOid: entraOid);
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        this.entraChangeNameServiceMock
            .Setup(x => x.ChangeNameAsync(entraOid, "Jane", "Smith", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var endpoint = this.CreateEndpoint();
        var request = new ChangeNameRequest { FirstName = "Jane", LastName = "Smith" };

        // Act
        var result = await endpoint.HandleAsync(userId, request, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<Ok>(result);

        var updatedUser = await this.dbContext.Users.SingleAsync(x => x.Sub == userId);
        Assert.AreEqual("Jane", updatedUser.FirstName);
        Assert.AreEqual("Smith", updatedUser.LastName);

        this.entraChangeNameServiceMock.Verify(x => x.ChangeNameAsync(entraOid, "Jane", "Smith", It.IsAny<CancellationToken>()), Times.Once);
        this.auditWriterMock.Verify(x => x.Log(It.IsAny<WriteToAuditRequest>()), Times.Once);
        this.eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<UserUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_WhenEntraUserFails_RollsBackDb_DoesNotAuditOrPublishEvent_AndReturnsProblem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraOid = Guid.NewGuid();
        var user = CreateTestUser(userId, "John", "Doe", isEntra: true, entraOid: entraOid);
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        this.entraChangeNameServiceMock
            .Setup(x => x.ChangeNameAsync(entraOid, "Jane", "Smith", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(EntraNameErrors.UserUpdateFailed("Graph API failed.")));

        var endpoint = this.CreateEndpoint();
        var request = new ChangeNameRequest { FirstName = "Jane", LastName = "Smith" };

        // Act
        var result = await endpoint.HandleAsync(userId, request, CancellationToken.None);

        // Assert
        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
        Assert.AreEqual("Failed to update user name in Entra: Graph API failed.", problemResult.ProblemDetails.Detail);

        // Verify DB rollback
        var revertedUser = await this.dbContext.Users.SingleAsync(x => x.Sub == userId);
        Assert.AreEqual("John", revertedUser.FirstName);
        Assert.AreEqual("Doe", revertedUser.LastName);

        // Verify no success audit and no published events
        this.auditWriterMock.Verify(x => x.Log(It.IsAny<WriteToAuditRequest>()), Times.Never);
        this.eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<UserUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
