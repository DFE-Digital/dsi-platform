using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.TestHelpers.Integration;
using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

[Collection("InternalApiIntegrationTestCollection")]
public abstract class InternalApiIntegrationEndpointTestBase(InternalApiWebApplicationFactory webAppFactory) : IAsyncLifetime
{
    private readonly List<IAsyncDisposable> createdFactories = [];

    protected InternalApiWebApplicationFactory WebAppFactory { get; } = webAppFactory;

    public async Task InitializeAsync()
    {
        await this.WebAppFactory.ResetDatabasesAsync();
    }

    public Task DisposeAsync()
    {
        return this.DisposeCreatedFactoriesAsync();
    }

    protected (HttpClient Client, CapturingWriteToAuditInteractor AuditMock) CreateClientWithAuditMock()
    {
        var auditMock = new CapturingWriteToAuditInteractor();

        var customisedFactory = this.WebAppFactory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IInteractor<WriteToAuditRequest>>();
                services.AddSingleton<IInteractor<WriteToAuditRequest>>(auditMock);
            });
        });

        this.createdFactories.Add(customisedFactory);

        var client = customisedFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.EnableAuthHeaderName, bool.TrueString);

        return (client, auditMock);
    }

    protected async Task InsertEntityAsync<TContext, TEntity>(TEntity entity)
        where TContext : DbContext
        where TEntity : class
    {
        await this.InsertEntitiesAsync<TContext, TEntity>([entity]);
    }

    protected async Task InsertEntitiesAsync<TContext, TEntity>(params TEntity[] entities)
        where TContext : DbContext
        where TEntity : class
    {
        await using var scope = this.WebAppFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private async Task DisposeCreatedFactoriesAsync()
    {
        foreach (var createdFactory in this.createdFactories) {
            await createdFactory.DisposeAsync();
        }

        this.createdFactories.Clear();
    }

    protected HttpClient CreateClient()
    {
        return this.WebAppFactory.CreateClient();
    }

    //protected HttpClient CreateAuthenticatedClient(string? userId = null, string? userName = null, params string[] roles)
    //{
    //    return this.WebAppFactory.CreateAuthenticatedClient(userId, userName, roles);
    //}

    //protected HttpClient CreateAnonymousClient()
    //{
    //    return this.WebAppFactory.CreateAnonymousClient();
    //}
}
