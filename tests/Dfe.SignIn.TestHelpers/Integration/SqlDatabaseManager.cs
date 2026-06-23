
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;

namespace Dfe.SignIn.TestHelpers.Integration;

/// <summary>
/// Describes a database catalog to provision inside the SQL container.
/// </summary>
/// <param name="CatalogName">The SQL catalog name (e.g., "dsi-directories-test").</param>
/// <param name="ConfigKey">
/// The configuration key prefix used to set EntityFramework__[ConfigKey]__Host/Name/Username/Password
/// environment variables (e.g., "Directories").
/// </param>
/// <param name="DbContextType">The EF Core <see cref="DbContext"/> type to run EnsureCreated on.</param>
public sealed record DatabaseCatalog(string CatalogName, string ConfigKey, Type DbContextType);

/// <summary>
/// Manages a single SQL Server Testcontainer, creates database catalogs,
/// initialises schemas via EF Core, and provides Respawn-based state resets.
/// </summary>
public sealed class SqlDatabaseManager : IAsyncDisposable
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly Dictionary<string, string> _connectionStrings = [];
    private readonly Dictionary<string, Respawner> _respawners = [];

    /// <summary>
    /// Starts the container and builds catalog connection strings.
    /// </summary>
    public async Task StartAsync(IReadOnlyList<DatabaseCatalog> catalogs)
    {
        await this._container.StartAsync();

        var containerConnectionString = this._container.GetConnectionString();

        foreach (var catalog in catalogs) {
            var csBuilder = new SqlConnectionStringBuilder(containerConnectionString) {
                InitialCatalog = catalog.CatalogName
            };
            this._connectionStrings[catalog.ConfigKey] = csBuilder.ConnectionString;
        }
    }

    /// <summary>
    /// Sets environment variables for each catalog so that the host's
    /// <c>EntityFramework:{ConfigKey}:Host/Name/Username/Password</c> configuration
    /// resolves correctly from the container's dynamic connection string.
    /// </summary>
    public void SetConnectionEnvironmentVariables()
    {
        foreach (var (configKey, cs) in this._connectionStrings) {
            var connectionStringBuilder = new SqlConnectionStringBuilder(cs);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Host", connectionStringBuilder.DataSource);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Name", connectionStringBuilder.InitialCatalog);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Username", connectionStringBuilder.UserID);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Password", connectionStringBuilder.Password);
        }
    }

    /// <summary>
    /// Initialises database schemas via EF Core EnsureCreated and builds Respawners.
    /// </summary>
    public async Task InitialiseSchemasAsync(IReadOnlyList<DatabaseCatalog> catalogs, IServiceProvider serviceProvider)
    {
        foreach (var catalog in catalogs) {
            var catalogConnectionString = this._connectionStrings[catalog.ConfigKey];

            using var scope = serviceProvider.CreateScope();
            var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(catalog.DbContextType);
            await dbContext.Database.EnsureCreatedAsync();

            using var connection = new SqlConnection(catalogConnectionString);
            await connection.OpenAsync();

            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions {
                DbAdapter = DbAdapter.SqlServer
            });

            this._respawners[catalog.ConfigKey] = respawner;
        }
    }

    /// <summary>
    /// Resets all databases back to empty schemas using Respawn.
    /// </summary>
    public async Task ResetAllAsync()
    {
        foreach (var (configKey, respawner) in this._respawners) {
            var catalogConnectionString = this._connectionStrings[configKey];
            await using var connection = new SqlConnection(catalogConnectionString);
            await connection.OpenAsync();
            await respawner.ResetAsync(connection);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await this._container.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
