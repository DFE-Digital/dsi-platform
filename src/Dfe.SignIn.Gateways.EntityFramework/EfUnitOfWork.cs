using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Implements the <see cref="IUnitOfWork"/> pattern using Entity Framework Core.
/// Manages a shared <see cref="DbContext"/> instance and provides access to typed repositories.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext dbContext;
    private readonly Dictionary<Type, object> repositories = [];

    /// <summary>
    /// Creates a new <see cref="EfUnitOfWork"/> using the provided <see cref="DbContext"/>.
    /// </summary>
    /// <param name="dbContext">The Entity Framework database context to use.</param>
    public EfUnitOfWork(DbContext dbContext)
    {
        this.dbContext = dbContext;
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

        var newRepo = new EfRepository<TEntity>(this.dbContext);
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
        await this.dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return this.dbContext.Database.CurrentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return this.dbContext.Database.CurrentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.dbContext.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await this.dbContext.DisposeAsync();
    }
}
