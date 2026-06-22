using System.Data.Common;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.IntegrationTests.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly Respawner _directoriesRespawner;
    private readonly Respawner _organisationsRespawner;

    private readonly string _directoriesConnectionString;
    private readonly string _organisationsConnectionString;

    public InternalApiWebApplicationFactory()
    {
        // 1. Set environment to Local so Program.cs uses user secrets / local bypasses
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");

        // 2. Load static JSON configurations and write them as environment variables (so they are visible immediately to builder.Configuration)
        LoadStaticConfigurations();

        // 3. Start the SQL Server container synchronously
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        var containerConnectionString = _dbContainer.GetConnectionString();

        // 4. Build specific catalog connection strings
        var directoriesBuilder = new SqlConnectionStringBuilder(containerConnectionString)
        {
            InitialCatalog = "dsi-directories-test"
        };
        _directoriesConnectionString = directoriesBuilder.ConnectionString;

        var organisationsBuilder = new SqlConnectionStringBuilder(containerConnectionString)
        {
            InitialCatalog = "dsi-organisations-test"
        };
        _organisationsConnectionString = organisationsBuilder.ConnectionString;

        // 5. Set dynamic connection variables so they are present in builder.Configuration immediately
        SetDynamicConnectionEnvironmentVariables();

        // 6. Initialize database schemas using EF Core EnsureCreated
        using var scope = Services.CreateScope();
        
        var directoriesContext = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        directoriesContext.Database.EnsureCreated();

        var organisationsContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
        organisationsContext.Database.EnsureCreated();

        // 7. Setup Respawner to clean database state between test runs
        _directoriesRespawner = Respawner.CreateAsync(_directoriesConnectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        }).GetAwaiter().GetResult();

        _organisationsRespawner = Respawner.CreateAsync(_organisationsConnectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        }).GetAwaiter().GetResult();
    }

    public async Task ResetDatabasesAsync()
    {
        await _directoriesRespawner.ResetAsync(_directoriesConnectionString);
        await _organisationsRespawner.ResetAsync(_organisationsConnectionString);
    }

    private void LoadStaticConfigurations()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
            .Build();

        foreach (var pair in config.AsEnumerable())
        {
            if (pair.Value is not null)
            {
                // Translate C# hierarchy separator ':' to process environment separator '__'
                var envKey = pair.Key.Replace(":", "__");
                Environment.SetEnvironmentVariable(envKey, pair.Value);
            }
        }
    }

    private void SetDynamicConnectionEnvironmentVariables()
    {
        // Directories Db Settings
        Environment.SetEnvironmentVariable("EntityFramework__Directories__Host", GetDataSource(_directoriesConnectionString));
        Environment.SetEnvironmentVariable("EntityFramework__Directories__Name", "dsi-directories-test");
        Environment.SetEnvironmentVariable("EntityFramework__Directories__Username", GetUserID(_directoriesConnectionString));
        Environment.SetEnvironmentVariable("EntityFramework__Directories__Password", GetPassword(_directoriesConnectionString));

        // Organisations Db Settings
        Environment.SetEnvironmentVariable("EntityFramework__Organisations__Host", GetDataSource(_organisationsConnectionString));
        Environment.SetEnvironmentVariable("EntityFramework__Organisations__Name", "dsi-organisations-test");
        Environment.SetEnvironmentVariable("EntityFramework__Organisations__Username", GetUserID(_organisationsConnectionString));
        Environment.SetEnvironmentVariable("EntityFramework__Organisations__Password", GetPassword(_organisationsConnectionString));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Inject TestAuthHandler to bypass real JWT checks
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }

    private static string GetDataSource(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString).DataSource;

    private static string GetUserID(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString).UserID;

    private static string GetPassword(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString).Password;

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
