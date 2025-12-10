using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Dfe.SignIn.Gateways.EntityFramework;

namespace migrations;

/// <summary>
/// A design-time factory for <see cref="DbDirectoriesContext"/> used by Entity Framework Core tools.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IDesignTimeDbContextFactory{TContext}"/> so that EF Core CLI
///   commands (such as adding migrations or updating the database) can create an instance of the
///   <see cref="DbDirectoriesContext"/> at design time, without requiring the application’s
///   dependency injection.</para>
/// </remarks>
public sealed class DirectoriesDbContextFactory : IDesignTimeDbContextFactory<DbDirectoriesContext>
{

    /// <summary>
    /// Creates a new instance of <see cref="DbDirectoriesContext"/> with design-time configuration.
    /// </summary>
    /// <param name="args">Command-line arguments passed by EF Core tools (not used in this implementation).</param>
    /// <returns>A configured instance of <see cref="DbDirectoriesContext"/>.</returns>
    public DbDirectoriesContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbDirectoriesContext>();
        optionsBuilder.UseSqlServer("", b => b.MigrationsAssembly("migrations"));

        return new DbDirectoriesContext(optionsBuilder.Options);
    }
}
