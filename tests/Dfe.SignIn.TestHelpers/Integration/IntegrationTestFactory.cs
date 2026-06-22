namespace Dfe.SignIn.TestHelpers.Integration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

/// <summary>
/// A reusable base <see cref="WebApplicationFactory{TEntryPoint}"/> for integration tests.
/// Manages SQL container lifecycle, configuration bridging, and database resets.
/// </summary>
/// <typeparam name="TProgram">
/// The entry point class of the API under test (e.g., <c>Program</c>).
/// </typeparam>
public abstract class IntegrationTestFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly SqlContainerFixture _sqlFixture = new();

    /// <summary>
    /// Defines the database catalogs that this API requires.
    /// Subclasses must override this to declare their catalogs.
    /// </summary>
    protected abstract IReadOnlyList<DatabaseCatalog> DatabaseCatalogs { get; }

    protected IntegrationTestFactory()
    {
        // 1. Set environment before the host builds
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");

        // 2. Load static config values from the test project's JSON file
        LoadStaticConfigurations();

        // 3. Start the SQL container
        _sqlFixture.StartAsync(DatabaseCatalogs).GetAwaiter().GetResult();

        // 4. Set connection env vars so the host can read them during build
        _sqlFixture.SetConnectionEnvironmentVariables();
    }

    /// <summary>
    /// Called after the host has been built and Services are available.
    /// Creates database schemas and initialises Respawners.
    /// Must be called explicitly from [ClassInitialize] or equivalent.
    /// </summary>
    public async Task InitialiseDatabasesAsync()
    {
        await _sqlFixture.InitialiseSchemasAsync(DatabaseCatalogs, Services);
    }

    /// <summary>
    /// Resets all databases to their empty schema state via Respawn.
    /// Call from [TestInitialize] or equivalent.
    /// </summary>
    public async Task ResetDatabasesAsync()
    {
        await _sqlFixture.ResetAllAsync();
    }

    /// <summary>
    /// Loads <c>appsettings.IntegrationTests.json</c> from the test project's output directory
    /// and writes each key-value pair as an environment variable using the <c>__</c> separator.
    /// </summary>
    private static void LoadStaticConfigurations()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
            .Build();

        foreach (var pair in config.AsEnumerable())
        {
            if (pair.Value is not null)
            {
                var envKey = pair.Key.Replace(":", "__");
                Environment.SetEnvironmentVariable(envKey, pair.Value);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await _sqlFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
