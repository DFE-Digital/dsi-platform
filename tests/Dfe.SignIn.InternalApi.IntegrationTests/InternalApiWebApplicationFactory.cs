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

    private Respawner? _directoriesRespawner;
    private Respawner? _organisationsRespawner;

    private string? _directoriesConnectionString;
    private string? _organisationsConnectionString;

    public async Task InitializeContainerAsync()
    {
        // Start the SQL Server container
        await _dbContainer.StartAsync();

        var containerConnectionString = _dbContainer.GetConnectionString();

        // Build specific catalog connection strings
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

        // Initialize database schemas using EF Core EnsureCreatedAsync
        using var scope = Services.CreateScope();
        
        var directoriesContext = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        await directoriesContext.Database.EnsureCreatedAsync();

        var organisationsContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
        await organisationsContext.Database.EnsureCreatedAsync();

        // Setup Respawner to clean database state between test runs
        _directoriesRespawner = await Respawner.CreateAsync(_directoriesConnectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        });

        _organisationsRespawner = await Respawner.CreateAsync(_organisationsConnectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        });
    }

    public async Task ResetDatabasesAsync()
    {
        if (_directoriesConnectionString != null && _directoriesRespawner != null)
        {
            await _directoriesRespawner.ResetAsync(_directoriesConnectionString);
        }
        if (_organisationsConnectionString != null && _organisationsRespawner != null)
        {
            await _organisationsRespawner.ResetAsync(_organisationsConnectionString);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Local to mock Service Bus, auditing, and other Azure endpoints
        builder.UseEnvironment("Local");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfiguration = new Dictionary<string, string?>
            {
                // Directories Db Settings
                ["EntityFramework:Directories:Host"] = GetDataSource(_directoriesConnectionString),
                ["EntityFramework:Directories:Name"] = "dsi-directories-test",
                ["EntityFramework:Directories:Username"] = GetUserID(_directoriesConnectionString),
                ["EntityFramework:Directories:Password"] = GetPassword(_directoriesConnectionString),

                // Organisations Db Settings
                ["EntityFramework:Organisations:Host"] = GetDataSource(_organisationsConnectionString),
                ["EntityFramework:Organisations:Name"] = "dsi-organisations-test",
                ["EntityFramework:Organisations:Username"] = GetUserID(_organisationsConnectionString),
                ["EntityFramework:Organisations:Password"] = GetPassword(_organisationsConnectionString),

                // Bypass Azure AD client credential throws
                ["InternalApiClient:Tenant"] = Guid.Empty.ToString(),
                ["InternalApiClient:ClientId"] = Guid.Empty.ToString(),
                ["InternalApiClient:ClientSecret"] = "dummy-secret",
                ["InternalApiClient:HostUrl"] = "https://localhost",
                ["InternalApiClient:BaseUrl"] = "https://localhost"
            };

            config.AddInMemoryCollection(testConfiguration);
        });

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

    private static string GetDataSource(string? connectionString) =>
        new SqlConnectionStringBuilder(connectionString).DataSource;

    private static string GetUserID(string? connectionString) =>
        new SqlConnectionStringBuilder(connectionString).UserID;

    private static string GetPassword(string? connectionString) =>
        new SqlConnectionStringBuilder(connectionString).Password;

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
