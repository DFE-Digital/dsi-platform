using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Dfe.SignIn.Gateways.EntityFramework;

namespace migrations;

/// <summary>
/// A design-time factory for <see cref="DbAuditContext"/> used by Entity Framework Core tools.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IDesignTimeDbContextFactory{TContext}"/> so that EF Core CLI
///   commands (such as adding migrations or updating the database) can create an instance of the
///   <see cref="DbAuditContext"/> at design time, without requiring the application’s
///   dependency injection.</para>
/// </remarks>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<DbAuditContext>
{

    /// <summary>
    /// Creates a new instance of <see cref="DbAuditContext"/> with design-time configuration.
    /// </summary>
    /// <param name="args">Command-line arguments passed by EF Core tools (not used in this implementation).</param>
    /// <returns>A configured instance of <see cref="DbAuditContext"/>.</returns>
    public DbAuditContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbAuditContext>();
        optionsBuilder.UseSqlServer("", b => b.MigrationsAssembly("migrations"));

        return new DbAuditContext(optionsBuilder.Options);
    }
}
