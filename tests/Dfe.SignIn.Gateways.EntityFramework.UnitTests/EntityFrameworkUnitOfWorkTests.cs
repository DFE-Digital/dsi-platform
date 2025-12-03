using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class EfUnitOfWorkTests
{
    private Mock<DbContext> mockContext = null!;
    private Mock<DatabaseFacade> mockDatabase = null!;
    private EntityFrameworkUnitOfWork uow = null!;

    public sealed class TestEntity { public int Id { get; set; } }

    public sealed class MockEntityFrameworkUnitOfWork : EntityFrameworkUnitOfWork
    {
        public MockEntityFrameworkUnitOfWork(DbContext dbContext, IEntityFrameworkTransactionContext transactionContext) : base(dbContext, transactionContext)
        {
        }
    }

    [TestInitialize]
    public void Setup()
    {
        this.mockContext = new Mock<DbContext>();
        this.mockDatabase = new Mock<DatabaseFacade>(this.mockContext.Object);
        this.mockContext.Setup(c => c.Database).Returns(this.mockDatabase.Object);

        this.uow = new MockEntityFrameworkUnitOfWork(this.mockContext.Object, new EntityFrameworkTransactionContext());
    }

    [TestMethod]
    public void Repository_ReturnsSameInstanceForSameEntityType()
    {
        var repo1 = this.uow.Repository<TestEntity>();
        var repo2 = this.uow.Repository<TestEntity>();

        Assert.AreSame(repo1, repo2);
    }

    [TestMethod]
    public async Task SaveChangesAsync_CallsDbContextSaveChangesAsync()
    {
        this.mockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await this.uow.SaveChangesAsync();

        this.mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task BeginTransactionAsync_CallsBeginTransactionAsync()
    {
        this.mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Mock.Of<IDbContextTransaction>());

        await this.uow.BeginTransactionAsync();

        this.mockDatabase.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CommitTransactionAsync_CallsCommitAsync()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        this.mockDatabase.Setup(d => d.CurrentTransaction).Returns(mockTransaction.Object);

        await this.uow.CommitTransactionAsync();

        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RollbackTransactionAsync_CallsRollbackAsync()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        this.mockDatabase.Setup(d => d.CurrentTransaction).Returns(mockTransaction.Object);

        await this.uow.RollbackTransactionAsync();

        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void Dispose_CallsDbContextDispose()
    {
        this.uow.Dispose();
        this.mockContext.Verify(c => c.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task DisposeAsync_CallsDbContextDisposeAsync()
    {
        this.mockContext
            .Setup(c => c.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        await this.uow.DisposeAsync();

        this.mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }
}
