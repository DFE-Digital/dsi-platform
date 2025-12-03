using Microsoft.EntityFrameworkCore;
using Moq;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class EfRepositoryTests
{
    private Mock<DbSet<TestEntity>> mockSet = null!;
    private Mock<DbContext> mockContext = null!;
    private EntityFrameworkRepository<TestEntity> repository = null!;
    private List<TestEntity> testData = null!;

    public sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [TestInitialize]
    public void Setup()
    {
        this.testData =
        [
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" },
            new TestEntity { Id = 3, Name = "Charlie" },
            new TestEntity { Id = 4, Name = "David" },
            new TestEntity { Id = 5, Name = "Eve" }
        ];

        var queryableData = this.testData.AsQueryable();

        this.mockSet = new Mock<DbSet<TestEntity>>();
        this.mockSet.As<IQueryable<TestEntity>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        this.mockSet.As<IQueryable<TestEntity>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        this.mockSet.As<IQueryable<TestEntity>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        this.mockSet.As<IQueryable<TestEntity>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator);

        this.mockContext = new Mock<DbContext>();
        this.mockContext.Setup(c => c.Set<TestEntity>()).Returns(this.mockSet.Object);

        this.repository = new EntityFrameworkRepository<TestEntity>(this.mockContext.Object);
    }

    [TestMethod]
    public void Where_FiltersCorrectly()
    {
        this.repository.Where(e => e.Id > 2);
        var results = this.repository.Query().ToList();

        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.All(e => e.Id > 2));
    }

    [TestMethod]
    public void SkipAndTake_WorkCorrectly()
    {
        this.repository.Skip(1).Take(2);
        var results = this.repository.Query().ToList();

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual(2, results[0].Id);
        Assert.AreEqual(3, results[1].Id);
    }

    [TestMethod]
    public void OrderBy_WorksCorrectly()
    {
        this.repository.OrderBy(e => e.Name);
        var results = this.repository.Query().ToList();

        Assert.AreEqual("Alice", results[0].Name);
        Assert.AreEqual("Bob", results[1].Name);
    }

    [TestMethod]
    public void OrderByDescending_WorksCorrectly()
    {
        this.repository.OrderByDescending(e => e.Name);
        var results = this.repository.Query().ToList();

        Assert.AreEqual("Eve", results[0].Name);
        Assert.AreEqual("David", results[1].Name);
    }

    [TestMethod]
    public void AddAsync_CallsDbSetAddAsync()
    {
        var newEntity = new TestEntity { Id = 6, Name = "Bob" };
        this.repository.AddAsync(newEntity);

        this.mockSet.Verify(m => m.AddAsync(newEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void Update_CallsDbContextUpdate()
    {
        var entity = this.testData[0];
        this.repository.Update(entity);

        this.mockContext.Verify(c => c.Update(entity), Times.Once);
    }

    [TestMethod]
    public void Delete_CallsDbContextRemove()
    {
        var entity = this.testData[0];
        this.repository.Delete(entity);

        this.mockContext.Verify(c => c.Remove(entity), Times.Once);
    }
}
