using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class EntityFrameworkRepositoryAndUnitOfWorkInMemoryTests
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
        await this.uow.AddAsync(entity);
        await this.uow.SaveChangesAsync();

        var retrieved = await repo.Where(x => x.Id == 1).FirstOrDefaultAsync();
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Red", retrieved.Colour);
    }

    [TestMethod]
    public async Task CanDeleteEntity()
    {
        var repo = this.uow.Repository<Hat>();
        var entity = new Hat { Id = 3, Colour = "Yellow" };
        await this.uow.AddAsync(entity);
        await this.uow.SaveChangesAsync();

        this.uow.Remove(entity);
        await this.uow.SaveChangesAsync();

        var deleted = await repo.Where(x => x.Id == 3).FirstOrDefaultAsync();
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task Skip_WorksCorrectly()
    {
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await this.uow.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        var results = await this.uow.Repository<Hat>()
                        .OrderBy(x => x.Id)
                        .Skip(1)
                        .ToListAsync();

        Assert.HasCount(2, results);
        Assert.AreEqual(2, results[0].Id);
        Assert.AreEqual(3, results[1].Id);
    }

    [TestMethod]
    public async Task Take_WorksCorrectly()
    {
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await this.uow.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        var results = await this.uow.Repository<Hat>()
                                .OrderBy(x => x.Id)
                                .Take(2)
                                .ToListAsync();

        Assert.HasCount(2, results);
        Assert.AreEqual(1, results[0].Id);
        Assert.AreEqual(2, results[1].Id);
    }

    [TestMethod]
    public async Task OrderBy_WorksCorrectly()
    {
        var entities = new List<Hat>
        {
            new() { Id = 1, Colour = "Red" },
            new() { Id = 2, Colour = "Blue" },
            new() { Id = 3, Colour = "Green" }
        };

        foreach (var e in entities) {
            await this.uow.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        var results = await this.uow.Repository<Hat>()
                                    .OrderBy(x => x.Colour)
                                    .ToListAsync();

        Assert.HasCount(3, results);
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
            await this.uow.AddAsync(e);
        }

        await this.uow.SaveChangesAsync();

        var results = await repo.Where(e => e.Id > 4)
            .OrderByDescending(e => e.Colour)
            .Skip(1)
            .Take(2)
            .ToListAsync();

        Assert.HasCount(2, results);
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
        await this.uow.AddAsync(entity);
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
        await this.uow.AddAsync(hat);
        await this.uow.SaveChangesAsync();

        repo.Include(h => h.Owner);
        var results = await repo.ToListAsync();

        Assert.HasCount(1, results);
        Assert.IsNotNull(results[0].Owner);
        Assert.AreEqual("Alice", results[0].Owner!.Name);
    }
}
