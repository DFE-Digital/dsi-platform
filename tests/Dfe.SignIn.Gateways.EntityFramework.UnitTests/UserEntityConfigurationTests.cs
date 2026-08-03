using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class UserEntityConfigurationTests
{
    // Note: this test uses the SQLite in-memory provider rather than the EF Core
    // InMemory provider. The InMemory provider does not enforce unique indexes
    // (confirmed: Microsoft.EntityFrameworkCore.InMemory.Storage.Internal.InMemoryTable<TKey>
    // only raises a concurrency-token check, never a uniqueness check), so a duplicate
    // insert would silently succeed there and this test would never be able to verify
    // the constraint added in UserEntityConfiguration. SQLite enforces UNIQUE constraints
    // for real, which is what's needed to prove the index behaves as intended.
    //
    // Scope limitation: this only proves exact-match uniqueness is enforced. SQLite's
    // in-memory TEXT unique index uses BINARY (case-sensitive) collation by default, not
    // SQL Server's SQL_Latin1_General_CP1_CI_AS (case-insensitive, accent-sensitive) used
    // by the live [user].email column. So this test cannot (and, without configuring a
    // SQLite collation in a way that wouldn't be a like-for-like check anyway, structurally
    // shouldn't try to) independently verify case-insensitive duplicate detection e.g.
    // "a@b.com" vs "A@B.com". That guarantee rests on the confirmed production column
    // collation (verified separately), not on this test.
    [TestMethod]
    public async Task Email_MustBeUnique()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseSqlite(connection)
            .Options;

        using var ctx = new DbDirectoriesContext(options);
        await ctx.Database.EnsureCreatedAsync();

        ctx.Users.Add(new UserEntity {
            Sub = Guid.NewGuid(),
            Email = "duplicate@example.com",
            FirstName = "Alex",
            LastName = "Johnson",
            Password = "",
            Salt = "",
            Status = 1,
        });
        await ctx.SaveChangesAsync();

        ctx.Users.Add(new UserEntity {
            Sub = Guid.NewGuid(),
            Email = "duplicate@example.com",
            FirstName = "Bob",
            LastName = "Simons",
            Password = "",
            Salt = "",
            Status = 1,
        });

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}
