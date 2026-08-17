using Dfe.SignIn.InternalApi.IntegrationTests.Mocks;
using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

[Collection("InternalApiIntegrationTestCollection")]
public abstract class InternalApiIntegrationEndpointTestBase(InternalApiWebApplicationFactory webAppFactory) : IAsyncLifetime
{
    protected InternalApiWebApplicationFactory WebAppFactory { get; } = webAppFactory;

    // Delegate to factory's shared singleton fakes
    // todo: could these just be removed and when needed access directly from factory?
    protected FakeInteractionLimiter FakeLimiter => this.WebAppFactory.FakeLimiter;
    protected FakeEmailRequestTracker FakeEmailRequestTracker => this.WebAppFactory.FakeEmailTracker;
    protected FakeExternalAuthService FakeExternalAuthService => this.WebAppFactory.FakeExternalAuth;
    protected FakeUserUpdatedPublisher FakeUserUpdatedPublisher => this.WebAppFactory.FakeUserUpdatedPublisher;
    protected CapturingWriteToAuditInteractor AuditCapturer => this.WebAppFactory.AuditCapturer;
    internal FakeTimestampInterceptor TimestampInterceptor => this.WebAppFactory.TimestampInterceptor;

    public async Task InitializeAsync()
    {
        await this.WebAppFactory.ResetDatabasesAsync();
        this.WebAppFactory.ResetAllFakes();
    }

    //todo: potentially remove this
    public Task DisposeAsync() => Task.CompletedTask;

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

    /// <summary>
    /// Creates an HttpClient directly from the root factory.
    /// Supports legacy pattern: this.CreateClient().WithAuthentication()
    /// </summary>
    protected HttpClient CreateClient() => this.WebAppFactory.CreateClient();
}
