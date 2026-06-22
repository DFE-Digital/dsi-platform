namespace Dfe.SignIn.TestHelpers.Integration;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;

/// <summary>
/// Manages a single SQL Server Testcontainer, creates database catalogs,
/// initialises schemas via EF Core, and provides Respawn-based state resets.
/// </summary>
public sealed class SqlContainerFixture : IAsyncDisposable
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly Dictionary<string, string> _connectionStrings = new();
    private readonly Dictionary<string, Respawner> _respawners = new();

    /// <summary>
    /// Starts the container and builds catalog connection strings.
    /// </summary>
    public async Task StartAsync(IReadOnlyList<DatabaseCatalog> catalogs)
    {
        await _container.StartAsync();

        var containerCs = _container.GetConnectionString();

        foreach (var catalog in catalogs)
        {
            var csBuilder = new SqlConnectionStringBuilder(containerCs)
            {
                InitialCatalog = catalog.CatalogName
            };
            _connectionStrings[catalog.ConfigKey] = csBuilder.ConnectionString;
        }
    }

    /// <summary>
    /// Sets environment variables for each catalog so that the host's
    /// <c>EntityFramework:{ConfigKey}:Host/Name/Username/Password</c> configuration
    /// resolves correctly from the container's dynamic connection string.
    /// </summary>
    public void SetConnectionEnvironmentVariables()
    {
        foreach (var (configKey, cs) in _connectionStrings)
        {
            var csb = new SqlConnectionStringBuilder(cs);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Host", csb.DataSource);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Name", csb.InitialCatalog);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Username", csb.UserID);
            Environment.SetEnvironmentVariable($"EntityFramework__{configKey}__Password", csb.Password);
        }
    }

    /// <summary>
    /// Initialises database schemas via EF Core EnsureCreated and builds Respawners.
    /// </summary>
    public async Task InitialiseSchemasAsync(IReadOnlyList<DatabaseCatalog> catalogs, IServiceProvider serviceProvider)
    {
        foreach (var catalog in catalogs)
        {
            var cs = _connectionStrings[catalog.ConfigKey];

            using var scope = serviceProvider.CreateScope();
            var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(catalog.DbContextType);
            await dbContext.Database.EnsureCreatedAsync();

            var respawner = await Respawner.CreateAsync(cs, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer
            });

            _respawners[catalog.ConfigKey] = respawner;
        }
    }

    /// <summary>
    /// Resets all databases back to empty schemas using Respawn.
    /// </summary>
    public async Task ResetAllAsync()
    {
        foreach (var (configKey, respawner) in _respawners)
        {
            var cs = _connectionStrings[configKey];
            await respawner.ResetAsync(cs);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
