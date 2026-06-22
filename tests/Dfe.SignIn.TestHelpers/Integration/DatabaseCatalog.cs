namespace Dfe.SignIn.TestHelpers.Integration;

/// <summary>
/// Describes a database catalog to provision inside the SQL container.
/// </summary>
/// <param name="CatalogName">The SQL catalog name (e.g., "dsi-directories-test").</param>
/// <param name="ConfigKey">
/// The configuration key prefix used to set EntityFramework__[ConfigKey]__Host/Name/Username/Password
/// environment variables (e.g., "Directories").
/// </param>
/// <param name="DbContextType">The EF Core <see cref="Microsoft.EntityFrameworkCore.DbContext"/> type to run EnsureCreated on.</param>
public sealed record DatabaseCatalog(
    string CatalogName,
    string ConfigKey,
    Type DbContextType);
