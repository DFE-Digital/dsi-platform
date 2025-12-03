namespace Dfe.SignIn.Core.Interfaces.DataAccess;

/// <summary>
/// Represents a unit of work that manages database transactions and provides
/// access to repositories. Ensures that a set of operations either fully succeed
/// or are rolled back as a single atomic operation.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type for which the repository is requested.
    /// </typeparam>
    /// <returns>
    ///   <para>An <see cref="IRepository{TEntity}"/> instance used to query or modify entities.</para>
    /// </returns>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;

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
public interface IDirectoriesUnitOfWork : IUnitOfWork { }

/// <summary>
/// A unit of work for the Organisations database.
/// </summary>
public interface IOrganisationsUnitOfWork : IUnitOfWork { }
