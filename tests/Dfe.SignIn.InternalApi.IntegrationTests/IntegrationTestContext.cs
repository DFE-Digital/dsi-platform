using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public sealed record IntegrationTestContext
{
    /// <summary>
    /// The HttpClient instance to use for making requests to the internal API.
    /// </summary>
    public required HttpClient Client { get; init; }

    /// <summary>
    /// Populated if .WithAuditMock() was called on the builder.
    /// </summary>
    public CapturingWriteToAuditInteractor? AuditMock { get; init; }

    /// <summary>
    /// Direct access to the email request tracker for verifying notifications.
    /// </summary>
    public FakeEmailRequestTracker? EmailTracker { get; init; }

    /// <summary>
    /// The DI container backing this client, reflecting any WithX(...) overrides applied.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>
    /// Inserts a single entity into the database using the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext to use for the operation.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to insert.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertEntityAsync<TContext, TEntity>(TEntity entity)
        where TContext : DbContext
        where TEntity : class
    {
        await this.InsertEntitiesAsync<TContext, TEntity>([entity]);
    }

    /// <summary>
    /// Inserts a collection of entities into the database using the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext to use for the operation.</typeparam>
    /// <typeparam name="TEntity">The type of the entities to insert.</typeparam>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertEntitiesAsync<TContext, TEntity>(IEnumerable<TEntity> entities)
        where TContext : DbContext
        where TEntity : class
    {
        await using var scope = this.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }
}
