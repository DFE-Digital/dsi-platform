using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class TimestampInterceptorTests
{
    private sealed class HatEntity
    {
        public required int Id { get; set; }
        public string Colour { get; set; } = "";
        public DateTime? CreatedAt { get; set; } = null;
        public DateTime? UpdatedAt { get; set; } = null;
    }

    private sealed class DateTimeOffsetEntity
    {
        public required int Id { get; set; }
        public string Colour { get; set; } = "";
        public DateTimeOffset? CreatedAt { get; set; } = null;
        public DateTimeOffset? UpdatedAt { get; set; } = null;
    }

    private sealed class TestDbContext : DbContext
    {
        private readonly TimeProvider timeProvider;
        public DbSet<HatEntity> Entities => this.Set<HatEntity>();
        public DbSet<DateTimeOffsetEntity> DateTimeOffsetEntities => this.Set<DateTimeOffsetEntity>();

        public TestDbContext(DbContextOptions<TestDbContext> options, TimeProvider timeProvider) : base(options)
        {
            this.timeProvider = timeProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new TimestampInterceptor(this.timeProvider));
        }
    }

    private static TestDbContext CreateContext(TimeProvider timeProvider)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options, timeProvider);
    }

    [TestMethod]
    public async Task AddedEntity_GetsCreatedAtAndUpdatedAt()
    {
        var timeProvider = new MockTimeProvider(new DateTimeOffset(2025, 12, 01, 01, 01, 01, TimeSpan.Zero));
        var ctx = CreateContext(timeProvider);
        var entity = new HatEntity {
            Id = 1,
            Colour = "blue"
        };

        ctx.Entities.Add(entity);
        await ctx.SaveChangesAsync();

        Assert.AreNotEqual(default, entity.CreatedAt);
        Assert.AreNotEqual(default, entity.UpdatedAt);
        Assert.IsTrue(entity.CreatedAt <= entity.UpdatedAt);
    }

    [TestMethod]
    public async Task ModifiedEntity_UpdatedAtChangesButCreatedAtDoesNot()
    {
        var timeProvider = new MockTimeProvider(new DateTimeOffset(2025, 12, 01, 01, 01, 01, TimeSpan.Zero));
        var ctx = CreateContext(timeProvider);
        var entity = new HatEntity {
            Id = 2
        };

        ctx.Entities.Add(entity);
        await ctx.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        entity.Colour = "red";
        ctx.Entities.Update(entity);
        timeProvider.Advance(TimeSpan.FromDays(1));

        await ctx.SaveChangesAsync();

        Assert.AreEqual(originalCreatedAt, entity.CreatedAt);
        Assert.IsTrue(entity.UpdatedAt > originalUpdatedAt);
    }

    [TestMethod]
    public async Task AddedEntity_GetsCreatedAtAndUpdatedAt_DateTimeOffset()
    {
        var timeProvider = new MockTimeProvider(new DateTimeOffset(2025, 12, 01, 01, 01, 01, TimeSpan.Zero));
        var ctx = CreateContext(timeProvider);
        var entity = new DateTimeOffsetEntity {
            Id = 1,
            Colour = "blue"
        };

        ctx.DateTimeOffsetEntities.Add(entity);
        await ctx.SaveChangesAsync();

        Assert.AreNotEqual(default, entity.CreatedAt);
        Assert.AreNotEqual(default, entity.UpdatedAt);
        Assert.IsTrue(entity.CreatedAt <= entity.UpdatedAt);
    }

    [TestMethod]
    public async Task ModifiedEntity_UpdatedAtChangesButCreatedAtDoesNot_DateTimeOffset()
    {
        var timeProvider = new MockTimeProvider(new DateTimeOffset(2025, 12, 01, 01, 01, 01, TimeSpan.Zero));
        var ctx = CreateContext(timeProvider);
        var entity = new DateTimeOffsetEntity {
            Id = 2
        };

        ctx.DateTimeOffsetEntities.Add(entity);
        await ctx.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        entity.Colour = "red";
        ctx.DateTimeOffsetEntities.Update(entity);
        timeProvider.Advance(TimeSpan.FromDays(1));

        await ctx.SaveChangesAsync();

        Assert.AreEqual(originalCreatedAt, entity.CreatedAt);
        Assert.IsTrue(entity.UpdatedAt > originalUpdatedAt);
    }
}
