using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Dfe.SignIn.Gateways.EntityFramework;

namespace migrations;

/// <summary>
/// A design-time factory for <see cref="DbOrganisationsContext"/> used by Entity Framework Core tools.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IDesignTimeDbContextFactory{TContext}"/> so that EF Core CLI
///   commands (such as adding migrations or updating the database) can create an instance of the
///   <see cref="DbOrganisationsContext"/> at design time, without requiring the application’s
///   dependency injection.</para>
/// </remarks>
public class OrganisationsDbContextFactory : IDesignTimeDbContextFactory<DbOrganisationsContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="DbOrganisationsContext"/> with design-time configuration.
    /// </summary>
    /// <param name="args">Command-line arguments passed by EF Core tools (not used in this implementation).</param>
    /// <returns>A configured instance of <see cref="DbOrganisationsContext"/>.</returns>
    public DbOrganisationsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbOrganisationsContext>();
        optionsBuilder.UseSqlServer("", b => b.MigrationsAssembly("migrations"));

        return new DbOrganisationsContext(optionsBuilder.Options);
    }
}
