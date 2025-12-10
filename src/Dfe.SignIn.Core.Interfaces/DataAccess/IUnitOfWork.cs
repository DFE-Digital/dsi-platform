namespace Dfe.SignIn.Core.Interfaces.DataAccess;

/// <summary>
/// Represents a unit of work that manages database transactions and provides
/// access to repositories. Ensures that a set of operations either fully succeed
/// or are rolled back as a single atomic operation.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Returns a queryable source for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type to be queried.
    /// </typeparam>
    /// <returns>
    /// An <see cref="IQueryable{TEntity}"/> that can be used to query or modify
    /// instances of <typeparamref name="TEntity"/>.
    /// </returns>
    IQueryable<TEntity> Repository<TEntity>() where TEntity : class;

    /// <summary>
    /// Asynchronously adds the specified entity to the current unit of work.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity being added.
    /// </typeparam>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Marks the specified entity for deletion in the current unit of work.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity being removed.
    /// </typeparam>
    /// <param name="entity">
    /// The entity instance to remove.
    /// </param>
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    ///   <para>The number of state entries written to the database.</para>
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the currently active database transaction.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the currently active database transaction.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// A unit of work for the Directories database.
/// </summary>
public interface IUnitOfWorkDirectories : IUnitOfWork { }

/// <summary>
/// A unit of work for the Organisations database.
/// </summary>
public interface IUnitOfWorkOrganisations : IUnitOfWork { }
