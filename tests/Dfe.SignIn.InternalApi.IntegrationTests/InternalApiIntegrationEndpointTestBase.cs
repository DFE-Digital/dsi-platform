using Dfe.SignIn.InternalApi.IntegrationTests.Mocks;
using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

[Collection("InternalApiIntegrationTestCollection")]
public abstract class InternalApiIntegrationEndpointTestBase(InternalApiWebApplicationFactory webAppFactory) : IAsyncLifetime
{
    private InternalApiWebApplicationFactory WebAppFactory { get; } = webAppFactory;

    // Delegate to factory's shared singleton fakes
    protected FakeInteractionLimiter FakeLimiter => this.WebAppFactory.FakeLimiter;
    protected FakeEmailRequestTracker FakeEmailRequestTracker => this.WebAppFactory.FakeEmailTracker;
    protected FakeExternalAuthService FakeExternalAuthService => this.WebAppFactory.FakeExternalAuth;
    protected FakeUserUpdatedPublisher FakeUserUpdatedPublisher => this.WebAppFactory.FakeUserUpdatedPublisher;
    protected CapturingWriteToAuditInteractor AuditCapturer => this.WebAppFactory.AuditCapturer;
    internal FakeTimestampInterceptor TimestampInterceptor => this.WebAppFactory.TimestampInterceptor;
    internal FailingDbCommandInterceptor FailingDbCommandInterceptor => this.WebAppFactory.FailingDbCommandInterceptor;

    public async Task InitializeAsync()
    {
        await this.WebAppFactory.ResetDatabasesAsync();
        this.WebAppFactory.ResetAllFakes();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Inserts a single entity into the specified database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task InsertEntityAsync<TContext, TEntity>(TEntity entity)
        where TContext : DbContext
        where TEntity : class
    {
        await this.InsertEntitiesAsync<TContext, TEntity>([entity]);
    }

    /// <summary>
    /// Inserts a collection of entities into the specified database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task InsertEntitiesAsync<TContext, TEntity>(IEnumerable<TEntity> entities)
        where TContext : DbContext
        where TEntity : class
    {
        await using var scope = this.WebAppFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Executes a query against the database context, returning the result.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <returns>The result of the query.</returns>
    protected async Task<TResult> ExecuteDbContextAsync<TContext, TResult>(Func<TContext, Task<TResult>> query)
        where TContext : DbContext
    {
        await using var scope = this.WebAppFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        return await query(dbContext);
    }

    /// <summary>
    /// Creates an HttpClient directly from the root factory.
    /// Supports legacy pattern: this.CreateClient().WithAuthentication()
    /// </summary>
    protected HttpClient CreateClient() => this.WebAppFactory.CreateClient();
}
