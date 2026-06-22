using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiWebApplicationFactory : IntegrationTestFactory<Program>
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
}
