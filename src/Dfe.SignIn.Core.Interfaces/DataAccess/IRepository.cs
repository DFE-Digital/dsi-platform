using System.Linq.Expressions;

namespace Dfe.SignIn.Core.Interfaces.DataAccess;

/// <summary>
/// Represents a generic repository that provides a fluent API for building
/// queries and performing CRUD operations on a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
public interface IRepository<TEntity>
{
    /// <summary>
    /// Returns the underlying <see cref="IQueryable{TEntity}"/> for the entity type.
    /// This allows additional LINQ operations to be composed externally if needed.
    /// </summary>
    /// <returns>
    ///   <para>An <see cref="IQueryable{TEntity}"/> representing the query.</para>
    /// </returns>
    IQueryable<TEntity> Query();

    /// <summary>
    /// Applies a filtering predicate to the query.
    /// </summary>
    /// <param name="predicate">A lambda expression used to filter the results.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Specifies a related entity to include in the query result.
    /// </summary>
    /// <typeparam name="TProperty">The type of the related entity.</typeparam>
    /// <param name="navigation">A lambda expression selecting the navigation property.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> navigation);

    /// <summary>
    /// Orders the query results by a specified key in ascending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the ordering key.</typeparam>
    /// <param name="keySelector">A lambda expression selecting the key.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    /// <summary>
    /// Orders the query results by a specified key in descending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the ordering key.</typeparam>
    /// <param name="keySelector">A lambda expression selecting the key.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> OrderByDescending<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    /// <summary>
    /// Skips the specified number of elements in the query.
    /// </summary>
    /// <param name="count">The number of elements to skip.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> Skip(int count);

    /// <summary>
    /// Limits the query to the specified number of elements.
    /// </summary>
    /// <param name="count">The maximum number of elements to take.</param>
    /// <returns>
    ///   <para>The current repository instance for method chaining.</para>
    /// </returns>
    IRepository<TEntity> Take(int count);

    /// <summary>
    /// Executes the query and returns the results as a list asynchronously.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    ///   <para>A list of <typeparamref name="TEntity"/>.</para>
    /// </returns>
    Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query and returns the first element or <c>null</c> if none exist.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    ///   <para>The first matching <typeparamref name="TEntity"/> or <c>null</c> if no entity matches.</para>
    /// </returns>
    Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity as updated.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);
}
