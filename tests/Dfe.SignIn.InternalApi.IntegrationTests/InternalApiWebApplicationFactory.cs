using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiWebApplicationFactory : IntegrationTestFactory<Program>, IAsyncLifetime
{
    public Action<IServiceCollection>? ConfigureAdditionalTestServices { get; set; }

    protected override IReadOnlyList<DatabaseCatalog> DatabaseCatalogs
    => [
        new("dsi-directories-test", "Directories", typeof(DbDirectoriesContext)),
        new("dsi-organisations-test", "Organisations", typeof(DbOrganisationsContext))
    ];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services => {
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            this.ConfigureAdditionalTestServices?.Invoke(services);
        });
    }

    public HttpClient CreateAuthenticatedClient(string? userId = null, string? userName = null, params string[] roles)
    {
        var client = this.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.EnableAuthHeaderName, bool.TrueString);

        if (!string.IsNullOrWhiteSpace(userId)) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId);
        }

        if (!string.IsNullOrWhiteSpace(userName)) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeaderName, userName);
        }

        foreach (var role in roles.Where(static r => !string.IsNullOrWhiteSpace(r))) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        }

        return client;
    }

    public HttpClient CreateAnonymousClient()
    {
        return this.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await this.InitialiseDatabasesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await this.DisposeAsync();
    }
}
