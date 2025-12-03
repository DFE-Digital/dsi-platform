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
    private readonly Dictionary<Type, object> repositories = [];

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
    /// Returns an <see cref="IRepository{TEntity}"/> instance for the specified entity type.
    /// This method ensures that only one repository is created per entity type by
    /// storing and reusing instances in an internal dictionary.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity the repository operates on.</typeparam>
    /// <returns>
    ///   <para>A cached or newly created <see cref="IRepository{TEntity}"/> instance for the entity type.</para>
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     The internal dictionary maps entity types to repository instances. This allows
    ///     the unit of work to provide a single shared repository per entity type,
    ///     ensuring that all repositories operate on the same <see cref="DbContext"/> instance.
    ///   </para>
    /// </remarks>
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (this.repositories.TryGetValue(entityType, out var repo)) {
            return (IRepository<TEntity>)repo;
        }

        var newRepo = new EntityFrameworkRepository<TEntity>(this.dbContext);
        this.repositories[entityType] = newRepo;

        return newRepo;
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
