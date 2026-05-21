using System.Linq.Expressions;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace Dfe.SignIn.Core.Repository.UnitTests;

[TestClass]
[Ignore]
public class OrganisationRepositoryTests
{
    private const string ClientName = "test-client";
    private readonly Guid UserId = Guid.NewGuid();

    private (OrganisationRepository repo, Mock<DbOrganisationsContext> dbContextMock) CreateMocks(
        List<GetUserOrganisationService> data)
    {
        // Mock DbSet via IQueryable
        var queryable = data.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<GetUserOrganisationService>(queryable);

        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .Options;

        var dbContextMock = new Mock<DbOrganisationsContext>(options);

        var databaseFacadeMock = new Mock<DatabaseFacade>(dbContextMock.Object);

        // Mock SqlQuery return
        dbContextMock
            .Setup(x => x.Database)
            .Returns(databaseFacadeMock.Object);

        databaseFacadeMock
            .Setup(x => x.SqlQuery<GetUserOrganisationService>(It.IsAny<FormattableString>()))
            .Returns(asyncEnumerable);

        var repo = new OrganisationRepository(dbContextMock.Object);

        return (repo, dbContextMock);
    }

    [TestMethod]
    public async Task ReturnsResults_WhenDataExists()
    {
        // Arrange
        List<GetUserOrganisationService> expected =
        [
            new()
            {
                UserId = this.UserId,
                ServiceName = "Service A",
                RoleName = "Admin"
            }
        ];

        var (repo, _) = this.CreateMocks(expected);

        // Act
        var result = await repo.SelectOrganisationServicesAndRolesByUserId(
            ClientName, this.UserId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        var item = result.First();
        Assert.AreEqual("Service A", item.ServiceName);
        Assert.AreEqual("Admin", item.RoleName);
    }

    [TestMethod]
    public async Task ReturnsEmpty_WhenNoData()
    {
        // Arrange
        var (repo, _) = this.CreateMocks([]);

        // Act
        var result = await repo.SelectOrganisationServicesAndRolesByUserId(
            ClientName, this.UserId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task CallsSqlQuery_WithCorrectParameters()
    {
        // Arrange
        var data = new List<GetUserOrganisationService>();

        var (repo, dbContextMock) = this.CreateMocks(data);

        // Act
        await repo.SelectOrganisationServicesAndRolesByUserId(
            ClientName, this.UserId, CancellationToken.None);

        // Assert
        dbContextMock.Verify(x =>
            x.Database.SqlQuery<GetUserOrganisationService>(
                It.Is<FormattableString>(sql =>
                    sql.ToString().Contains("WHERE") &&
                    sql.ToString().Contains("clientId") &&
                    sql.ToString().Contains("status")))
            , Times.Once);
    }

    [TestMethod]
    public async Task ReturnsMultipleRecords_WhenMultipleExist()
    {
        // Arrange

        List<GetUserOrganisationService> expected =
        [
            new() { ServiceName = "Service A", RoleName = "Admin" },
            new() { ServiceName = "Service B", RoleName = "User" }
        ];

        var (repo, _) = this.CreateMocks(expected);

        // Act
        var result = await repo.SelectOrganisationServicesAndRolesByUserId(
            ClientName, this.UserId, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, result.Count());
        CollectionAssert.AreEquivalent(
            new[] { "Service A", "Service B" },
            result.Select(x => x.ServiceName).ToArray());
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        this._inner = inner;
    }

    public T Current => this._inner.Current;

    public ValueTask DisposeAsync()
    {
        this._inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
        => ValueTask.FromResult(this._inner.MoveNext());

}
