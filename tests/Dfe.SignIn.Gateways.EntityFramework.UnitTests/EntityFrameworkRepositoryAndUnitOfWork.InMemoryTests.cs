using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class EntityFrameworkRepositoryAndUnitOfWorkInMemoryTests
{

    public sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Hat> Hats => this.Set<Hat>();
    }

    public sealed class Hat
    {
        public int Id { get; set; }
        public string Colour { get; set; } = "";
        public Person? Owner { get; set; }
    }

    public sealed class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class MockEntityFrameworkUnitOfWork : EntityFrameworkUnitOfWork
    {
        public MockEntityFrameworkUnitOfWork(DbContext dbContext, IEntityFrameworkTransactionContext transactionContext) : base(dbContext, transactionContext)
        {
        }
    }

    private TestDbContext context = null!;
    private EntityFrameworkUnitOfWork uow = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new TestDbContext(options);
        this.uow = new MockEntityFrameworkUnitOfWork(this.context, new EntityFrameworkTransactionContext());
    }

    [TestMethod]
    public async Task CanAddAndRetrieveEntity()
    {
        var repo = this.uow.Repository<Hat>();
        var entity = new Hat { Id = 1, Colour = "Red" };
        await repo.AddAsync(entity);
        await this.uow.SaveChangesAsync();

        var retrieved = await repo.Where(x => x.Id == 1).FirstOrDefaultAsync();
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Red", retrieved.Colour);
    }

    [TestMethod]
    public async Task CanUpdateEntity()
    {
        var repo = this.uow.Repository<Hat>();
        var entity = new Hat { Id = 2, Colour = "Blue" };
        await repo.AddAsync(entity);
        await this.uow.SaveChangesAsync();

        entity.Colour = "Green";
        repo.Update(entity);
        await this.uow.SaveChangesAsync();

        var updated = await repo.Where(x => x.Id == 2).FirstOrDefaultAsync();
        Assert.IsNotNull(updated);
        Assert.AreEqual("Green", updated.Colour);
    }

    [TestMethod]
    public async Task CanDeleteEntity()
    {
        var repo = this.uow.Repository<Hat>();
        var entity = new Hat { Id = 3, Colour = "Yellow" };
        await repo.AddAsync(entity);
        await this.uow.SaveChangesAsync();

        repo.Delete(entity);
        await this.uow.SaveChangesAsync();

        var deleted = await repo.Where(x => x.Id == 3).FirstOrDefaultAsync();
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task Skip_WorksCorrectly()
    {
        var repo = this.uow.Repository<Hat>();
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await repo.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        repo.Skip(1);
        var results = await repo.ToListAsync();

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual(2, results[0].Id);
        Assert.AreEqual(3, results[1].Id);
    }

    [TestMethod]
    public async Task Take_WorksCorrectly()
    {
        var repo = this.uow.Repository<Hat>();
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await repo.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        repo.Take(2);
        var results = await repo.ToListAsync();

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual(1, results[0].Id);
        Assert.AreEqual(2, results[1].Id);
    }

    [TestMethod]
    public async Task OrderBy_WorksCorrectly()
    {
        var repo = this.uow.Repository<Hat>();
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await repo.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        repo.OrderBy(e => e.Colour);
        var results = await repo.ToListAsync();

        Assert.AreEqual(3, results.Count);
        Assert.AreEqual("Blue", results[0].Colour);
        Assert.AreEqual("Green", results[1].Colour);
        Assert.AreEqual("Red", results[2].Colour);
    }

    [TestMethod]
    public async Task CanQueryWithWhere_Skip_Take_OrderBy_OrderByDescending()
    {
        var repo = this.uow.Repository<Hat>();
        var entities = new List<Hat>
        {
            new() { Id = 4, Colour = "Purple" },
            new() { Id = 5, Colour = "Orange" },
            new() { Id = 6, Colour = "Black" },
            new() { Id = 7, Colour = "White" },
        };

        foreach (var e in entities) {
            await repo.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        var results = await repo.Where(e => e.Id > 4)
            .OrderByDescending(e => e.Colour)
            .Skip(1)
            .Take(2)
            .ToListAsync();

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.All(e => e.Id > 4));
        Assert.AreEqual("Orange", results[0].Colour);
        Assert.AreEqual("Black", results[1].Colour);
    }

    [TestMethod]
    public void Repository_ReturnsSameInstanceForSameType()
    {
        var repo1 = this.uow.Repository<Hat>();
        var repo2 = this.uow.Repository<Hat>();

        Assert.AreSame(repo1, repo2);
    }

    [TestMethod]
    public async Task SaveChangesAsync_UpdatesDatabase()
    {
        var repo = this.uow.Repository<Hat>();
        var entity = new Hat { Id = 8, Colour = "Brown" };
        await repo.AddAsync(entity);
        var result = await this.uow.SaveChangesAsync();

        Assert.AreEqual(1, result);
        var retrieved = await repo.Where(x => x.Id == 8).FirstOrDefaultAsync();
        Assert.IsNotNull(retrieved);
    }

    [TestMethod]
    public async Task Include_WorksCorrectly()
    {
        var repo = this.uow.Repository<Hat>();

        var hat = new Hat {
            Id = 9,
            Colour = "Magenta",
            Owner = new Person { Id = 1, Name = "Alice" }
        };
        await repo.AddAsync(hat);
        await this.uow.SaveChangesAsync();

        repo.Include(h => h.Owner);
        var results = await repo.ToListAsync();

        Assert.AreEqual(1, results.Count);
        Assert.IsNotNull(results[0].Owner);
        Assert.AreEqual("Alice", results[0].Owner!.Name);
    }
}
