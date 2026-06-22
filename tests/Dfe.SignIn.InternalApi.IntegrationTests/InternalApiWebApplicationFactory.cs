using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiWebApplicationFactory : IntegrationTestFactory<Program>, IAsyncLifetime
{
    protected override IReadOnlyList<DatabaseCatalog> DatabaseCatalogs
    => [
        new("dsi-directories-test", "Directories", typeof(DbDirectoriesContext)),
        new("dsi-organisations-test", "Organisations", typeof(DbOrganisationsContext))
    ];

    protected override string AppSettingsFileName => "appsettings.Test.json";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services => {
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await InitialiseDatabasesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }
}
