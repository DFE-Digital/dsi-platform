using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.TestHelpers.Integration;
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

    protected abstract string AppSettingsFileName { get; }

    protected IntegrationTestFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        LoadStaticConfigurations(this.AppSettingsFileName);

        this._sqlFixture.StartAsync(this.DatabaseCatalogs).GetAwaiter().GetResult();
        this._sqlFixture.SetConnectionEnvironmentVariables();
    }

    /// <summary>
    /// Called after the host has been built and Services are available.
    /// Creates database schemas and initialises Respawners.
    /// Must be called explicitly from [ClassInitialize] or equivalent.
    /// </summary>
    public async Task InitialiseDatabasesAsync()
    {
        await this._sqlFixture.InitialiseSchemasAsync(this.DatabaseCatalogs, this.Services);
    }

    /// <summary>
    /// Resets all databases to their empty schema state via Respawn.
    /// Call from [TestInitialize] or equivalent.
    /// </summary>
    public async Task ResetDatabasesAsync()
    {
        await this._sqlFixture.ResetAllAsync();
    }

    /// <summary>
    /// Loads <c>appsettings.IntegrationTests.json</c> from the test project's output directory
    /// and writes each key-value pair as an environment variable using the <c>__</c> separator.
    /// </summary>
    private static void LoadStaticConfigurations(string appSettingsFileName)
    {
        if (string.IsNullOrEmpty(appSettingsFileName)) {
            return;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(appSettingsFileName, optional: false)
            .Build();

        foreach (var pair in config.AsEnumerable()) {
            if (pair.Value is not null) {
                var envKey = pair.Key.Replace(":", "__");
                Environment.SetEnvironmentVariable(envKey, pair.Value);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await this._sqlFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
