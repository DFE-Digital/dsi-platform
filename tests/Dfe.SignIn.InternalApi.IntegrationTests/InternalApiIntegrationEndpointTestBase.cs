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

    /// <summary>
    /// This method creates a new HttpClient instance with a mock implementation of the IInteractor<WriteToAuditRequest> interface.
    /// The mock implementation captures the WriteToAuditRequest passed to it, allowing for verification of audit logging behavior during integration tests.
    /// NOTE: This has only been added to support the interator that writes to audit, so that we can verify that the correct audit events are being written during integration tests.
    /// NOTE: When we move to a simpler IAuditorService implementation, this method can be removed and the tests can be updated to use the real implementation of IAuditorService.
    /// </summary>
    /// <returns></returns>
    protected (HttpClient Client, CapturingWriteToAuditInteractor AuditMock) CreateClientWithAuditMock()
    {
        var auditMock = new CapturingWriteToAuditInteractor();
        var auditWriterMock = new TestAuditWriter(auditMock);

        var customisedFactory = this.WebAppFactory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IInteractor<WriteToAuditRequest>>();
                services.AddSingleton<IInteractor<WriteToAuditRequest>>(auditMock);

                services.RemoveAll<IAuditWriter>();
                services.AddSingleton<IAuditWriter>(auditWriterMock);
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

    protected async Task InsertEntitiesAsync<TContext, TEntity>(IEnumerable<TEntity> entities)
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
        var customisedFactory = this.WebAppFactory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IInteractor<WriteToAuditRequest>>();
                services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();

                services.RemoveAll<IAuditWriter>();
                services.AddSingleton<IAuditWriter, TestAuditWriter>();
            });
        });

        this.createdFactories.Add(customisedFactory);

        return customisedFactory.CreateClient();
    }
}
