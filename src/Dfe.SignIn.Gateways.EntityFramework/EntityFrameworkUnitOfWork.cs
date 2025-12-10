using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Implements the <see cref="IUnitOfWork"/> pattern using Entity Framework Core.
/// Manages a shared <see cref="DbContext"/> instance and provides access to typed repositories.
/// </summary>
public abstract class EntityFrameworkUnitOfWork : IUnitOfWork
{
    private readonly DbContext dbContext;
    private readonly IEntityFrameworkTransactionContext transactionContext;

    /// <summary>
    /// Creates a new <see cref="EntityFrameworkUnitOfWork"/> using the provided <see cref="DbContext"/>.
    /// </summary>
    /// <param name="dbContext">The Entity Framework database context to use.</param>
    /// <param name="transactionContext">The Entity Framework core transaction context.</param>
    protected EntityFrameworkUnitOfWork(DbContext dbContext, IEntityFrameworkTransactionContext transactionContext)
    {
        this.dbContext = dbContext;
        this.transactionContext = transactionContext;
    }

    /// <summary>
    /// Returns an <see cref="IQueryable{TEntity}"/> representing the underlying
    /// Entity Framework Core <see cref="DbSet{TEntity}"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type for which the queryable repository is requested.
    /// </typeparam>
    /// <returns>
    ///   <para>An <see cref="IQueryable{TEntity}"/> that can be used to query and modify
    ///   entities of type <typeparamref name="TEntity"/>.
    ///   Changes made to tracked entities are persisted when the unit of work
    ///   is committed.</para>
    /// </returns>
    public IQueryable<TEntity> Repository<TEntity>() where TEntity : class
    {
        return this.dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    /// <remarks>
    ///   <para>This method queues an insert operation by adding the entity to the DbContext.
    ///    No database interaction occurs until <see cref="SaveChangesAsync"/> is called.</para>
    /// </remarks>
    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        await this.dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    ///   <para>This method queues a delete operation by marking the entity as deleted in the DbContext.
    ///   No database interaction occurs until <see cref="SaveChangesAsync"/> is called.</para>
    /// </remarks>
    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        this.dbContext.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return this.dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        this.transactionContext.InsideTransaction = true;
        await this.dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        this.transactionContext.InsideTransaction = false;
        return this.dbContext.Database.CurrentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        this.transactionContext.InsideTransaction = false;
        return this.dbContext.Database.CurrentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core dispose logic for EF Core resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            this.dbContext?.Dispose();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core async dispose logic for EF Core resources.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (this.dbContext != null) {
            await this.dbContext.DisposeAsync();
        }
    }
}
